import React, { useContext } from "react";
import { UserContext } from "../components/Contexts";
import UserData from "../models/UserData";
import AccountService from "../services/AccountService";

export default function Login() {
  const context = useContext(UserContext);

  function PerformLogin(e: React.SyntheticEvent<HTMLFormElement>) {
    e.preventDefault();
    const formData = new FormData(e.currentTarget);

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
