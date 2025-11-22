import { useContext, type JSX } from "react";
import { Navigate, Outlet } from "react-router-dom";
import { UserContext } from "../Contexts";
import Constants from "../enum/Constants";

export default function LoginChecker(): JSX.Element {
  const userContext = useContext(UserContext);

  if (userContext.user?.AccountId) {
    return <Outlet />;
  }

  return <Navigate to={Constants.LoginRoute} replace />;
}
