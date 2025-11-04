import { useContext, type JSX } from "react";
import { Navigate, Outlet } from "react-router-dom";
import { UserContext } from "./Contexts";

export default function LoginChecker(): JSX.Element {
  const userContext = useContext(UserContext);

  if (userContext.user?.AccountId) {
    return <Outlet />;
  }

  return <Navigate to="/login" replace />;
}
