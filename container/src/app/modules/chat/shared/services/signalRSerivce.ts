import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@aspnet/signalr';
import { Message } from '../models/entities/message.model';
import { DataService } from './dataService';
import { ContactService } from './contactService';
import { MessageCto } from '../models/dto/messageCto';
@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  private hubConnection: HubConnection
  public user: any;
  constructor(
    private dataService: DataService,
    private contactService: ContactService) {
    this.user = JSON.parse(localStorage.getItem('currentUser'));
    this.connect();
  }

  public connect()
  {
    this.hubConnection = new HubConnectionBuilder()
                            .withUrl('https://chateduc.w12.hoster.by/chat')
                            .withAutomaticReconnect()
                            .build();
    this.hubConnection
      .start()
      .then(() => {
        console.log('server start signalR')
        this.join(this.user.id, this.user.role);
        this.addChatListener();
      })
      .catch(err => console.log('Error while starting connection: ' + err))
  }

  public addChatListener() {
    this.hubConnection.on('GetMessage', (message: Message) => {
      this.dataService.AddMsg(message);
    });

    this.hubConnection.on('Status', (userId: number, status: boolean) => {
      this.dataService.SetStatus(userId,status);
      this.contactService.SetStatus(userId,status);
    });

    this.hubConnection.on('RemovedMessage', (chatId: any, msgId: any) => {
      this.dataService.RemoveMsg(chatId, msgId);
    })

    this.hubConnection.on('EditedMessage',(chatId: any, msgId: any,text:any)=>{
      this.dataService.updateMsg(chatId,msgId,text);
    })

    this.hubConnection.on('NewChat',(firstId: any, secondId: any,chatId:any)=>{
      this.contactService.updateChats(firstId,secondId,chatId);
    }) 
  }

  public addChat(firstId: number, secondId: number, chatId:number)
  {
    return this.hubConnection.invoke("AddChat", firstId, secondId,chatId);
  }


  public updateGroupMessage(id:number,text:string,chatId:number)
  {
    return this.hubConnection.invoke("UpdateGroupMessage", id, text,chatId);
  }


  public updateChatMessage(id:number,text:string,chatId:number)
  {
    return this.hubConnection.invoke("UpdateChatMessage", id, text,chatId);
  }

  public sendMessage(msg: MessageCto)  {
    return this.hubConnection.invoke("SendMessage", this.user.id, JSON.stringify(msg))
  }

  public sendGroupMessage(msg: MessageCto) {
    return this.hubConnection.invoke("SendGroupMessage", this.user.id, this.user.role, JSON.stringify(msg))
  }

  public SendGroupFiles(files) {
    const k = 1024;
    const dm = 2;
    const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB', 'PB', 'EB', 'ZB', 'YB'];
    const formData = new FormData();
    for (var item of files) {
      var i = Math.floor(Math.log(item.size) / Math.log(k));
      var msg = new MessageCto();
      msg.userId = this.user.id;
      msg.chatId = this.dataService.activChatId;
      msg.fileSize = parseFloat((item.size / Math.pow(k, i)).toFixed(dm)) + ' ' + sizes[i];
      msg.fileContent = item.name;
      if (item.name.includes(".jpg") || item.name.includes(".png")) {
        msg.isimage = true;
      }
      else
        msg.isfile = true;
      formData.append(item.name, item);
    }
    formData.append("ChatId", this.dataService.activChatId.toString());
    this.dataService.SendImg(formData).subscribe(result=>  this.sendGroupMessage(msg))
  }

  public remove(id: any) {
    if (this.dataService.isGroupChat)
      return this.hubConnection.invoke("DeleteGroupMsg", id.toString(), this.dataService.activChatId.toString());
    else
      return this.hubConnection.invoke("DeleteChatMsg", id.toString(), this.dataService.activChatId.toString());    
    }

  public join(userId: number, role: string) {
    return this.hubConnection.invoke("Join", userId, role);
  }
}
