import React, { useContext, type JSX } from "react";
import { Navigate } from "react-router-dom";
import { UserContext } from "../components/Contexts";
import "../css/Login.css";
import AccountService from "../services/AccountService";

export default function Login(): JSX.Element {
  const context = useContext(UserContext);

  function PerformLogin(e: React.SyntheticEvent<HTMLFormElement>): void {
    e.preventDefault();
    const formData = new FormData(e.currentTarget);

    AccountService.Login(formData)
      .then((data) => {
        if (context.setUser) {
          context.setUser(data);
        }
      })
      .catch((error) => console.log(error));
  }

  if (context.user?.AccountId) {
    return <Navigate to="/" />;
  }

  return (
    <div>
      <div className="tear"></div>
      <div className="ripple"></div>
      <form className="login" onSubmit={PerformLogin}>
        <input className="login-input" type="text" name="username" />
        <br />
        <input className="login-input" type="password" name="password" />
        <br />
        <button className="login-button" type="submit">
          Login
        </button>
      </form>
    </div>
  );
}
