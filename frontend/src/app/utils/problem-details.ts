import { HttpErrorResponse } from '@angular/common/http';

import { ProblemDetails } from '../models/track.model';

/** Turns an API error (RFC 7807 ProblemDetails) into a single readable message. */
export function problemMessage(error: unknown, fallback: string): string {
  // 0 = no response; 502/503/504 = the dev proxy could not reach the API.
  if (error instanceof HttpErrorResponse && [0, 502, 503, 504].includes(error.status)) {
    return "Can't reach the API. Make sure the backend is running (run.cmd or dotnet run).";
  }

  if (!(error instanceof HttpErrorResponse) || !error.error || typeof error.error !== 'object') {
    return fallback;
  }

  const problem = error.error as ProblemDetails;
  const fieldErrors = problem.errors ? Object.values(problem.errors).flat() : [];
  if (fieldErrors.length > 0) return fieldErrors.join(' ');

  return problem.detail || problem.title || fallback;
}
