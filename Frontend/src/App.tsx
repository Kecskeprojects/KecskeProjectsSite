import { useEffect, useState, type JSX } from "react";
import { ImSpinner2 } from "react-icons/im";
import { BrowserRouter, Navigate, Route, Routes } from "react-router-dom";
import LoginChecker from "./auth/LoginChecker";
import RoleChecker from "./auth/RoleChecker";
import Notification from "./components/Notification";
import "./css/App.css";
import Constants from "./enum/Constants";
import Layout from "./layout/Layout";
import UserData from "./models/UserData";
import ExampleComponent from "./pages/ExampleComponent";
import Home from "./pages/Home";
import Login from "./pages/Login";
import AccountService from "./services/AccountService";
import LogTools from "./tools/LogTools";
import { UserContext } from "./utilities/Contexts";

export default function App(): JSX.Element {
  const [user, setUser] = useState<UserData>();
  const [loading, setLoading] = useState<boolean>(true);

  useEffect(() => {
    if (user?.AccountId) {
      return;
    }

    AccountService.GetLoggedInUser()
      .then((data) => {
        setUser(data);
        setLoading(false);
      })
      .catch((error) => LogTools.setErrorNotification(error?.message ?? error));
  }, []);

  if (loading) {
    return (
      <ImSpinner2 className="animate-spin" size={"50vh"} color="rgb(0,0,0)" />
    );
  }

  return (
    <UserContext.Provider value={{ user, setUser }}>
      <BrowserRouter>
        <Routes>
          <Route element={<Notification />}>
            <Route path={Constants.LoginRoute} element={<Login />} />
            <Route element={<LoginChecker />}>
              <Route path="/" element={<Layout />}>
                <Route index element={<Home />} />
                <Route element={<RoleChecker roles={["Admin"]} />}>
                  <Route path="example" element={<ExampleComponent />}>
                    <Route path=":id" element={<ExampleComponent />} />
                  </Route>
                </Route>
                <Route path="*" element={<Navigate to="/" replace />} />
              </Route>
            </Route>
          </Route>
        </Routes>
      </BrowserRouter>
    </UserContext.Provider>
  );
}
