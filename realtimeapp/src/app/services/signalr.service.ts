import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Observable, Subject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class SignalrService {
  private hubConnection!: signalR.HubConnection;
  private _data$: Subject<string> = new Subject<string>();
  public data$: Observable<string> = this._data$.asObservable();

  constructor() { }

  public startConnection = () => {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('http://localhost:5053/datahub', {
        skipNegotiation: true,
        transport: signalR.HttpTransportType.WebSockets
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection
      .start()
      .then(() => console.log('SignalR Connection started!'))
      .catch(err => console.log('Error while starting SignalR connection: ' + err));

    this.hubConnection.onclose(async () => {
      console.log('SignalR Connection closed. Attempting to reconnect...');
      await this.startConnection(); // Attempt to restart connection on close
    });
  }

  public addDataListener = () => {
    this.hubConnection.on('ReceiveData', (data) => {
      console.log('Received data from SignalR:', data);
      this._data$.next(data); // Push the received data to our observable
    });
  }
}
