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

  // Functional updates, so several notifications raised before a re-render are all kept
  function addToArray(message: INotificationMessage) {
    setNotification((current) =>
      current.some((x) => x.message === message.message)
        ? current
        : [...current, message],
    );
  }

  function removeFromArray() {
    setNotification((current) => current.slice(1));
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
