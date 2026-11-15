import { useContext, useEffect, type JSX } from "react";
import { Navigate, useSearchParams } from "react-router-dom";
import EditWindowBase from "../components/window/EditWindowBase";
import LoginWindowDescription from "../components/window/WindowDescriptions/LoginWindowDescription";
import "../css/Login.css";
import UserData from "../models/UserData";
import { UserContext } from "../utilities/Contexts";
import LogTools from "../tools/LogTools";

export default function Login(): JSX.Element {
  const context = useContext(UserContext);
  const [searchParams, setSearchParams] = useSearchParams();

  useEffect(() => {
    // Check if user was redirected due to session expiration
    if (searchParams.get("expired") === "true") {
      LogTools.setInfoNotification("Your session has expired. Please log in again.");

      // Remove the flag so a page refresh doesn't show the message again
      setSearchParams(
        (params) => {
          params.delete("expired");
          return params;
        },
        { replace: true },
      );
    }
  }, [searchParams, setSearchParams]);

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
