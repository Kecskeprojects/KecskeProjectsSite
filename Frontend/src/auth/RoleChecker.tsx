import { useContext, type JSX } from "react";
import { Navigate, Outlet } from "react-router-dom";
import { UserContext } from "../utilities/Contexts";

export type RoleCheckerProps = {
  roles?: Array<string>;
};

export default function RoleChecker(props: RoleCheckerProps): JSX.Element {
  const userContext = useContext(UserContext);

  if (userContext.user?.hasRoles(props.roles)) {
    return <Outlet />;
  }

  return <Navigate to="/" replace />;
}
