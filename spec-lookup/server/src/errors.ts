export class AppError extends Error {
  constructor(
    public readonly statusCode: number,
    public readonly code: string,
    message: string,
    public readonly details?: unknown,
  ) {
    super(message);
  }
}

export const notFound = (what = 'Product') => new AppError(404, 'NOT_FOUND', `${what} not found.`);
export const badRequest = (message: string, details?: unknown) => new AppError(400, 'VALIDATION_ERROR', message, details);
export const unauthorized = () => new AppError(401, 'UNAUTHORIZED', 'Please log in to continue.');
export const forbidden = (message = 'You do not have permission to do this.') => new AppError(403, 'FORBIDDEN', message);
export const conflict = (code: string, message: string, details?: unknown) => new AppError(409, code, message, details);
