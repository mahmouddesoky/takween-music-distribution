import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

import { TrackService } from '../../services/track.service';
import { AuthService } from '../../services/auth.service';
import { Track, TrackStatus } from '../../models/track.model';

const STATUS_OPTIONS: Array<{ label: string; value: TrackStatus | '' }> = [
  { label: 'All', value: '' },
  { label: 'Draft', value: 'Draft' },
  { label: 'Submitted', value: 'Submitted' },
  { label: 'Distributed', value: 'Distributed' },
];

@Component({
  selector: 'app-track-list',
  imports: [CommonModule, RouterLink],
  templateUrl: './track-list.html',
  styleUrl: './track-list.css',
})
export class TrackListComponent implements OnInit {
  private readonly trackService = inject(TrackService);
  private readonly auth = inject(AuthService);

  readonly isLoggedIn = this.auth.isLoggedIn;
  readonly tracks = signal<Track[]>([]);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly selectedStatus = signal<TrackStatus | ''>('');

  readonly statusOptions = STATUS_OPTIONS;

  ngOnInit(): void {
    this.loadTracks();
  }

  onStatusChange(status: TrackStatus | ''): void {
    this.selectedStatus.set(status);
    this.loadTracks();
  }

  private loadTracks(): void {
    this.loading.set(true);
    this.error.set(null);

    this.trackService.getTracks({ status: this.selectedStatus() }).subscribe({
      next: tracks => {
        this.tracks.set(tracks);
        this.loading.set(false);
      },
      error: () => {
        this.error.set("Couldn't load tracks. Check the API and try again.");
        this.loading.set(false);
      },
    });
  }
}
