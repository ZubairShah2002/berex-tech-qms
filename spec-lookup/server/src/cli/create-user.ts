/**
 * Creates a user or resets an existing user's password from the command line.
 * Usage: npm run create-user -- <userId> <admin|qc> [--import]
 * The password is read from the NEW_PASSWORD environment variable.
 * With the built-in database, stop the app before running this.
 */
import { openDatabase, migrate } from '../db.js';
import { hashPassword, passwordProblem } from '../auth.js';

const [userId, role] = process.argv.slice(2);
const canImport = process.argv.includes('--import');
const password = process.env.NEW_PASSWORD ?? '';
if (!userId || (role !== 'admin' && role !== 'qc')) {
  console.error('Usage: NEW_PASSWORD=... npm run create-user -- <userId> <admin|qc> [--import]');
  process.exit(1);
}
const problem = passwordProblem(password);
if (problem) { console.error(`NEW_PASSWORD: ${problem}`); process.exit(1); }

// With the built-in database, stop the app first: only one program may open the data folder.
const pool = await openDatabase();
await migrate(pool);
const hash = await hashPassword(password);
const { rows } = await pool.query(
  `INSERT INTO users (user_id, display_name, password_hash, role, can_import) VALUES ($1, $1, $2, $3, $4)
   ON CONFLICT (lower(user_id)) DO UPDATE SET password_hash = EXCLUDED.password_hash, role = EXCLUDED.role,
     can_import = EXCLUDED.can_import, status = 'active', updated_at = now()
   RETURNING (xmax = 0) AS inserted`,
  [userId, hash, role, canImport || role === 'admin']);
console.log(rows[0].inserted ? `Created ${role} user ${userId}.` : `Updated user ${userId} (password reset, role ${role}).`);
await pool.end();
