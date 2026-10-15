export type TrackStatus = 'Draft' | 'Submitted' | 'Distributed';
export type DistributionStatus = 'Pending' | 'Live' | 'Rejected';

export interface Dsp {
  id: number;
  name: string;
}

/** RFC 7807 error body returned by the API. */
export interface ProblemDetails {
  title?: string;
  detail?: string;
  status?: number;
  errors?: Record<string, string[]>;
}

export interface Artist {
  id: number;
  name: string;
  email: string;
  country: string;
}

/** Shape returned by GET /api/tracks (list view). */
export interface Track {
  id: number;
  title: string;
  artistId: number;
  artistName: string;
  isrc: string;
  releaseDate: string;
  genre: string;
  status: TrackStatus;
}

/** Shape returned by GET /api/tracks/:id (detail view). */
export interface TrackDetail extends Track {
  distributions: TrackDistribution[];
}

/** Distribution entry — note the flat dspName, matching the backend TrackDetailResponse. */
export interface TrackDistribution {
  dspId: number;
  dspName: string;
  status: DistributionStatus;
  submittedAt: string;
}

export interface TrackFilters {
  artistId?: number;
  genre?: string;
  status?: TrackStatus | '';
}

export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  expiresAt: string;
}

export interface CreateTrackRequest {
  title: string;
  artistId: number;
  isrc: string;
  releaseDate: string;
  genre: string;
  status?: string;
}
