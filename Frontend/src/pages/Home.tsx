import { useContext } from "react";
import { Link } from "react-router-dom";
import { UserContext } from "../components/Contexts";
import RoleEnum from "../enum/RoleEnum";

export default function Home() {
  const context = useContext(UserContext);

  return (
    <div>
      Home Page
      <div>
        <Link to="example">Example Page</Link>
        <br />
        {context.user?.hasRole(RoleEnum.Admin) ? "IsAdmin" : "IsNotAdmin"}
      </div>
    </div>
  );
}
