import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

import { Artist, CreateTrackRequest, Dsp, Track, TrackDetail, TrackFilters } from '../models/track.model';

@Injectable({ providedIn: 'root' })
export class TrackService {
  private readonly http = inject(HttpClient);
  private readonly tracksUrl = '/api/tracks';
  private readonly artistsUrl = '/api/artists';
  private readonly dspsUrl = '/api/dsps';

  getTracks(filters: TrackFilters = {}): Observable<Track[]> {
    let params = new HttpParams();

    if (filters.artistId) {
      params = params.set('artistId', filters.artistId);
    }
    if (filters.genre) {
      params = params.set('genre', filters.genre);
    }
    if (filters.status) {
      params = params.set('status', filters.status);
    }

    return this.http.get<Track[]>(this.tracksUrl, { params });
  }

  getTrack(id: number | string): Observable<TrackDetail> {
    return this.http.get<TrackDetail>(`${this.tracksUrl}/${id}`);
  }

  createTrack(req: CreateTrackRequest): Observable<TrackDetail> {
    return this.http.post<TrackDetail>(this.tracksUrl, req);
  }

  distributeTrack(id: number, dspIds: number[]): Observable<TrackDetail> {
    return this.http.post<TrackDetail>(`${this.tracksUrl}/${id}/distribute`, { dspIds });
  }

  getArtists(): Observable<Artist[]> {
    return this.http.get<Artist[]>(this.artistsUrl);
  }

  getDsps(): Observable<Dsp[]> {
    return this.http.get<Dsp[]>(this.dspsUrl);
  }
}
