import { useContext, type JSX } from "react";
import { Navigate, Outlet } from "react-router-dom";
import type IRoleCheckerProps from "../interface/IRoleCheckerProps";
import { UserContext } from "../utilities/Contexts";

export default function RoleChecker(props: IRoleCheckerProps): JSX.Element {
  const userContext = useContext(UserContext);

  if (userContext.user?.hasRoles(props.roles)) {
    return <Outlet />;
  }

  return <Navigate to="/" replace />;
}
