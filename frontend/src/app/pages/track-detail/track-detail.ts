import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';

import { TrackService } from '../../services/track.service';
import { AuthService } from '../../services/auth.service';
import { TrackDetail } from '../../models/track.model';

@Component({
  selector: 'app-track-detail',
  imports: [CommonModule, RouterLink],
  templateUrl: './track-detail.html',
  styleUrl: './track-detail.css',
})
export class TrackDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly trackService = inject(TrackService);
  private readonly auth = inject(AuthService);

  readonly track = signal<TrackDetail | null>(null);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  // Distribution form state (admin only)
  readonly isLoggedIn = this.auth.isLoggedIn;
  readonly allDsps = signal<Array<{ id: number; name: string }>>([
    { id: 1, name: 'Spotify' },
    { id: 2, name: 'Apple Music' },
    { id: 3, name: 'YouTube' },
  ]);
  readonly selectedDspIds = signal<number[]>([]);
  readonly distributing = signal(false);
  readonly distributeMsg = signal<string | null>(null);
  readonly distributeErr = signal<string | null>(null);

  readonly availableDsps = computed(() => {
    const t = this.track();
    if (!t) return this.allDsps();
    const distributed = new Set(t.distributions.map(d => d.dspId));
    return this.allDsps().filter(dsp => !distributed.has(dsp.id));
  });

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.error.set('No track id provided.');
      return;
    }
    this.loadTrack(id);
  }

  toggleDsp(id: number): void {
    const current = this.selectedDspIds();
    this.selectedDspIds.set(
      current.includes(id) ? current.filter(x => x !== id) : [...current, id]
    );
  }

  isDspSelected(id: number): boolean {
    return this.selectedDspIds().includes(id);
  }

  distribute(): void {
    const t = this.track();
    if (!t) return;
    const ids = this.selectedDspIds();
    if (ids.length === 0) return;

    this.distributing.set(true);
    this.distributeErr.set(null);
    this.distributeMsg.set(null);

    this.trackService.distributeTrack(t.id, ids).subscribe({
      next: updated => {
        this.track.set(updated);
        this.selectedDspIds.set([]);
        this.distributing.set(false);
        this.distributeMsg.set('Track submitted to the selected DSPs.');
      },
      error: () => {
        this.distributing.set(false);
        this.distributeErr.set('Distribution failed. The track may already be submitted to one of those DSPs.');
      },
    });
  }

  private loadTrack(id: string): void {
    this.loading.set(true);
    this.error.set(null);

    this.trackService.getTrack(id).subscribe({
      next: track => {
        this.track.set(track);
        this.loading.set(false);
      },
      error: () => {
        this.error.set("Couldn't load this track. It may not exist.");
        this.loading.set(false);
      },
    });
  }
}
