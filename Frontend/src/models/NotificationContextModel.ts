import type INotificationMessage from "../interface/INotificationMessage";

export default class NotificationContextModel {
  notification?: INotificationMessage;
  setNotification?: (notification: INotificationMessage) => void;

  constructor(setNotification?: (notification: INotificationMessage) => void) {
    this.setNotification = setNotification;
  }
}
