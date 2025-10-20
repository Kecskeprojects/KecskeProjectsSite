import { useContext } from "react";
import { Link } from "react-router-dom";
import { UserContext } from "../components/Contexts";

export default function Home() {
  const context = useContext(UserContext);

  return (
    <div>
      Home Page
      <div>
        <Link to="example">Example Page</Link>
        <br />
        {context.user.hasRole("Admin") ? "IsAdmin" : "IsNotAdmin"}
      </div>
    </div>
  );
}
