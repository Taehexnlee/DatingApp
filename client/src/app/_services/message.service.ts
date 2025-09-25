import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { PaginatedResult } from '../_models/pagination';
import { Message } from '../_models/message';
import { setPaginatedResponse, setPaginationHeaders } from './paginationHelper';
import { HubConnection, HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { User } from '../_models/user';
import { Group } from '../_models/group';
import { BusyService } from './busy.service';

@Injectable({
  providedIn: 'root'
})
export class MessageService {
  apiUrl = environment.apiUrl;
  hubsUrl = environment.hubsUrl;

  private http = inject(HttpClient);
  private busyService   = inject(BusyService)
  hubConnection?: HubConnection;

  paginatedResult = signal<PaginatedResult<Message[]> | null>(null);
  messageThread = signal<Message[]>([]);

  createHubConnection(user: User, otherUsername: string) {
    this.busyService.busy();
    if (this.hubConnection?.state === HubConnectionState.Connected) {
      console.log('Hub already connected');
      return;
    }

    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this.hubsUrl + 'message?user=' + otherUsername, {
        accessTokenFactory: () => user.token
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.start()
      .then(() => console.log('✅ Hub connection started'))
      .catch(error => console.log('Hub connection error:', error));

    this.hubConnection.on('ReceiveMessageThread', messages => {
      console.log('📩 Received message thread:', messages);
      this.messageThread.set(messages);
    });

    this.hubConnection.on('NewMessage', message => {
      this.messageThread.update(messages => [...messages, message]);
    });

    this.hubConnection.on('UpdatedGroup', (group: Group) => {
      if (group.connections?.some(x => x.username === otherUsername)) {
        this.messageThread.update(messages =>
          messages.map(m => {
            if (!m.dateRead) {
              return { ...m, dateRead: new Date(Date.now()) };
            }
            return m;
          })
        );
      }
    });
  }

  stopHubConenction() {
    if (this.hubConnection?.state === HubConnectionState.Connected) {
      this.hubConnection.stop()
        .then(() => console.log('🛑 Hub disconnected'))
        .catch(error => console.log('Hub stop error:', error));
    }
  }

  getMessages(pageNumber: number, pageSize: number, container: string) {
    let params = setPaginationHeaders(pageNumber, pageSize);
    params = params.append('Container', container);

    return this.http.get<Message[]>(this.apiUrl + 'messages', { observe: 'response', params })
      .subscribe({
        next: response => setPaginatedResponse(response, this.paginatedResult)
      });
  }

  getMessageThread(username: string) {
    return this.http.get<Message[]>(this.apiUrl + 'messages/thread/' + username);
  }

  async sendMessage(username: string, content: string) {
    if (!this.hubConnection || this.hubConnection.state !== HubConnectionState.Connected) {
      console.warn('❗ Message send failed: SignalR connection not established');
      return;
    }

    try {
      await this.hubConnection.invoke('SendMessage', {
        recipientUsername: username,
        content
      });
    } catch (error) {
      console.error('❗ Error while sending message:', error);
    }
  }

  deleteMessage(id: number) {
    return this.http.delete(this.apiUrl + 'messages/' + id);
  }
}
