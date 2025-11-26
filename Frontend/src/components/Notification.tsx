import { useContext, type JSX } from "react";
import { Outlet } from "react-router-dom";
import { NotificationContext } from "../utilities/Contexts";

export default function Notification(): JSX.Element {
  const context = useContext(NotificationContext);

  return (
    <>
      {context.notification?.notificationType ? <div></div> : null}
      <Outlet />
    </>
  );
}
