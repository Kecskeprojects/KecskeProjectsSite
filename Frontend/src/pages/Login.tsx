import { useContext, type JSX } from "react";
import { Navigate } from "react-router-dom";
import EditWindowBase from "../components/window/EditWindowBase";
import LoginWindowDescription from "../components/window/WindowDescriptions/LoginWindowDescription";
import "../css/Login.css";
import UserData from "../models/UserData";
import { UserContext } from "../utilities/Contexts";

export default function Login(): JSX.Element {
  const context = useContext(UserContext);

  function handleLoginResponse(content: any): void {
    if (content instanceof UserData && context.setUser) {
      context.setUser(content);
    }
  }

  if (context.user?.AccountId) {
    return <Navigate to="/" />;
  }

  return (
    <div>
      <div className="tear"></div>
      <div className="ripple"></div>
      <EditWindowBase
        responseHandler={handleLoginResponse}
        windowDescriptionClass={LoginWindowDescription}
        key={"loginWindow"}
      />
    </div>
  );
}
