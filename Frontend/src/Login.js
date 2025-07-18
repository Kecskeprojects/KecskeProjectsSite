import { useContext } from "react";
import { UserContext } from "./Contexts";
import UserData from "./Models/UserData";
import AccountService from "./Services/AccountService";

export default function Login() {
  const context = useContext(UserContext);

  function PerformLogin(e) {
    e.preventDefault();
    const formData = new FormData(e.target);

    AccountService.Login(formData)
      .then((data) => {
        context.setUser(new UserData(data));
      })
      .catch((error) => console.log(error));
  }

  return (
    <div>
      <form onSubmit={PerformLogin}>
        <input type="text" name="username" />
        <br />
        <input type="password" name="password" />
        <br />
        <button type="submit">Login</button>
      </form>
    </div>
  );
}
