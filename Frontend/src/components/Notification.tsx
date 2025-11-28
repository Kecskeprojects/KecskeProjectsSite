import { useState, type JSX } from "react";
import { Outlet } from "react-router-dom";
import "../css/Notification.css";
import type INotificationMessage from "../interface/INotificationMessage";
import LogTools from "../tools/LogTools";

export default function Notification(): JSX.Element {
  LogTools.setNotification = addToArray;
  const [notification, setNotification] = useState<Array<INotificationMessage>>(
    []
  );

  const currentNotification = notification?.length > 0 ? notification[0] : null;

  function addToArray(message: INotificationMessage) {
    if (notification.some((x) => x.message === message.message)) {
      return;
    }
    setNotification([...notification, message]);
  }

  function removeFromArray() {
    setNotification(notification.slice(1, notification.length - 1));
  }

  const classNames =
    "notification-base notification-animation" +
    (currentNotification?.notificationType
      ? ` ${currentNotification?.notificationType}`
      : "");

  return (
    <>
      {currentNotification !== null ? (
        <div
          key={currentNotification.notificationId}
          onAnimationEnd={removeFromArray}
          className={classNames}
        >
          {currentNotification.message}
        </div>
      ) : null}
      <Outlet />
    </>
  );
}
