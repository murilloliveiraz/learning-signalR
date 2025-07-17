import { Component, OnInit, OnDestroy } from '@angular/core';
import { SignalrService } from './services/signalr.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit, OnDestroy {
  title = 'Real-time Data Dashboard';
  receivedMessages: string[] = [];
  private dataSubscription!: Subscription;

  constructor(public signalrService: SignalrService) { }

  ngOnInit(): void {
    this.signalrService.startConnection();
    this.signalrService.addDataListener();

    this.dataSubscription = this.signalrService.data$.subscribe(message => {
      this.receivedMessages.push(message);
      if (this.receivedMessages.length > 20) {
        this.receivedMessages.shift();
      }
    });
  }

  ngOnDestroy(): void {
    if (this.dataSubscription) {
      this.dataSubscription.unsubscribe();
    }
  }
}