import Constants from "../enum/Constants";
import NotificationTypeEnum from "../enum/NotificationTypeEnum";
import type INotificationMessage from "../interface/INotificationMessage";
import EnvironmentTools from "./EnvironmentTools";

export default class LogTools {
  static DebugLog(header: string, message: any) {
    if (!EnvironmentTools.IsProduction()) {
      console.log(Constants.DebugSeparator);
      console.log(header);
      console.log(message);
      console.log(Constants.DebugSeparator);
    }
  }

  // Replaced by the Notification component once it renders, this is only the fallback before that
  static setNotification(message: INotificationMessage) {
    if (!message || !message.message || !message.notificationType) {
      return;
    }
    alert(message.message);
  }

  static setErrorNotification(message: string): void {
    LogTools.setNotification({
      message: message,
      notificationType: NotificationTypeEnum.Error,
      notificationId: new Date().getTime(),
    });
  }

  static setSuccessNotification(message: string): void {
    LogTools.setNotification({
      message: message,
      notificationType: NotificationTypeEnum.Success,
      notificationId: new Date().getTime(),
    });
  }

  static setInfoNotification(message: string): void {
    LogTools.setNotification({
      message: message,
      notificationType: NotificationTypeEnum.Info,
      notificationId: new Date().getTime(),
    });
  }
}
