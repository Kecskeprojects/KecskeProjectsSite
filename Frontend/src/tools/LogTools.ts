import Constants from "../enum/Constants";
import NotificationTypeEnum from "../enum/NotificationTypeEnum";
import type INotificationMessage from "../interface/INotificationMessage";

export default class LogTools {
  static DebugLog(header: string, message: any) {
    console.log(Constants.DebugSeparator);
    console.log(header);
    console.log(message);
    console.log(Constants.DebugSeparator);
  }

  static setNotification(message: INotificationMessage) {
    if (!message || !message.message || !message.notificationType) {
      return;
    }
    alert("Not implemented! " + message);
  }

  static setErrorNotification(message: string): void {
    this.setNotification({
      message: message,
      notificationType: NotificationTypeEnum.Error,
      notificationId: new Date().getTime(),
    });
  }

  static setSuccessNotification(message: string): void {
    this.setNotification({
      message: message,
      notificationType: NotificationTypeEnum.Success,
      notificationId: new Date().getTime(),
    });
  }

  static setInfoNotification(message: string): void {
    this.setNotification({
      message: message,
      notificationType: NotificationTypeEnum.Info,
      notificationId: new Date().getTime(),
    });
  }
}
