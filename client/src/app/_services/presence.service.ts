import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import {HubConnection, HubConnectionBuilder, HubConnectionState} from '@microsoft/signalr'
import { ToastrService } from 'ngx-toastr';
import { User } from '../_models/user';
import { take } from 'rxjs';
import { Router } from '@angular/router';
@Injectable({
  providedIn: 'root'
})
export class PresenceService {

  hubUrl = environment.hubsUrl;
  private hubConnections?: HubConnection;
  private toastr = inject(ToastrService);
  private router = inject(Router);
  onlineUsers = signal<string[]>([]);

  createHubConnection(user: User) {
    this.hubConnections = new HubConnectionBuilder()
      .withUrl(this.hubUrl + 'presence', {
        accessTokenFactory:() => user.token
      })
      .withAutomaticReconnect()
      .build();
      
      this.hubConnections.start().catch(error => console.log(error));
      this.hubConnections.on('UserIsOnline', username => {
        this.onlineUsers.update(users => [...users, username])
      })
      this.hubConnections.off('UserIsOffline', username => {
        this.onlineUsers.update(users => users.filter(x => x !== username))
      });
      this .hubConnections.on('GetOnlineUsers', usernames => {
        this.onlineUsers.set(usernames)
      })
      this.hubConnections.on('NewMessageReceived', ({username, knownAs}) => {
        this.toastr.info(knownAs + ' has sent you a new message! Click me to see it')
          .onTap
          .pipe(take(1))
          .subscribe(() => this.router.navigateByUrl('/members/' + username + '?tab=Messages') )
      })
   }
   stopHubConnection() {
    if(this.hubConnections?.state === HubConnectionState.Connected)
    {
      this.hubConnections.stop().catch(error => console.log(error))
    }
   }
}
