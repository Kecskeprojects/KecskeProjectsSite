import { useContext, type JSX } from "react";
import { Navigate, Outlet } from "react-router-dom";
import { UserContext } from "./Contexts";

export default function ProtectedRoute(props: {
  roles?: Array<string>;
}): JSX.Element | undefined {
  const userContext = useContext(UserContext);

  if (userContext.user?.hasRoles(props.roles)) {
    return <Outlet />;
  }

  return <Navigate to="/" replace />;
}
