import type { ReactNode } from 'react';

export interface ColumnDef<T> {
  key: keyof T & string;
  label: string;
  width: string;
  required?: boolean;
  placeholder?: string;
  multiline?: boolean;
  list?: string;
}

/**
 * Editable table of rows with add / delete / move up / move down.
 * Shown as a table on desktop and as stacked fields on phones.
 */
export function RowsEditor<T extends { id?: string }>({
  rows, columns, onChange, empty, addLabel, idPrefix, showErrors,
}: {
  rows: T[];
  columns: ColumnDef<T>[];
  onChange: (rows: T[]) => void;
  empty: () => T;
  addLabel: string;
  idPrefix: string;
  showErrors?: boolean;
}) {
  const template = `${columns.map((c) => c.width).join(' ')} 124px`;
  const update = (i: number, key: keyof T, value: string) =>
    onChange(rows.map((r, j) => (j === i ? { ...r, [key]: value } : r)));
  const move = (i: number, d: number) => {
    const j = i + d;
    if (j < 0 || j >= rows.length) return;
    const next = [...rows];
    [next[i], next[j]] = [next[j], next[i]];
    onChange(next);
  };
  const remove = (i: number) => onChange(rows.filter((_, j) => j !== i));
  const add = () => {
    onChange([...rows, empty()]);
    // Focus the first field of the new row.
    setTimeout(() => document.getElementById(`${idPrefix}-${rows.length}-${columns[0].key}`)?.focus(), 0);
  };

  let body: ReactNode;
  if (rows.length === 0) {
    body = <div className="panel-empty ns">None added.</div>;
  } else {
    body = rows.map((row, i) => (
      <div className="row-edit" style={{ gridTemplateColumns: template }} key={row.id ?? i}>
        {columns.map((c) => {
          const id = `${idPrefix}-${i}-${c.key}`;
          const value = (row[c.key] as unknown as string | null) ?? '';
          const invalid = showErrors && c.required && !value.trim();
          return (
            <div key={c.key}>
              <label className="cell-label" htmlFor={id}>{c.label}{c.required && <span className="req"> *</span>}</label>
              {c.multiline ? (
                <textarea id={id} className="textarea" style={{ minHeight: 34 }} rows={1} value={value}
                  placeholder={c.placeholder} onChange={(e) => update(i, c.key, e.target.value)} />
              ) : (
                <input id={id} className="input" value={value} placeholder={c.placeholder} list={c.list}
                  aria-invalid={invalid || undefined} aria-label={c.label}
                  onChange={(e) => update(i, c.key, e.target.value)} />
              )}
            </div>
          );
        })}
        <div className="row-actions">
          <button type="button" className="btn btn-sm btn-icon" onClick={() => move(i, -1)} disabled={i === 0} aria-label="Move up" title="Move up">↑</button>
          <button type="button" className="btn btn-sm btn-icon" onClick={() => move(i, 1)} disabled={i === rows.length - 1} aria-label="Move down" title="Move down">↓</button>
          <button type="button" className="btn btn-sm btn-danger" onClick={() => remove(i)} aria-label="Delete row" title="Delete">Delete</button>
        </div>
      </div>
    ));
  }

  return (
    <div className="rows-editor">
      {rows.length > 0 && (
        <div className="rows-head" style={{ gridTemplateColumns: template }}>
          {columns.map((c) => <div key={c.key}>{c.label}{c.required && <span className="req"> *</span>}</div>)}
          <div />
        </div>
      )}
      {body}
      <div className="rows-foot">
        <button type="button" className="btn" onClick={add}>+ {addLabel}</button>
      </div>
    </div>
  );
}
