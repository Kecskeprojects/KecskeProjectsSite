import { useEffect, useState } from "react";
import { ImSpinner2 } from "react-icons/im";
import { BrowserRouter, Navigate, Route, Routes } from "react-router-dom";
import { UserContext } from "./components/Contexts";
import ProtectedRoute from "./components/ProtectedRoute";
import "./css/App.css";
import Layout from "./layout/Layout";
import UserData from "./models/UserData";
import ExampleComponent from "./pages/ExampleComponent";
import Home from "./pages/Home";
import Login from "./pages/Login";
import AccountService from "./services/AccountService";

export default function App() {
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
      .catch((error) => console.log(error));
  }, []);

  if (loading) {
    return (
      <div
        className="animate-spin"
        style={{ margin: "200px auto auto auto", width: "min-content" }}
      >
        <ImSpinner2 className="animate-spin" size={500} color="rgb(0,0,0)" />
      </div>
    );
  }

  return (
    <UserContext.Provider value={{ user, setUser }}>
      {!user?.AccountId ? (
        <Login />
      ) : (
        <BrowserRouter>
          <Routes>
            <Route path="/" element={<Layout />}>
              <Route index element={<Home />} />
              <Route
                path="example"
                element={
                  <ProtectedRoute
                    roles={["Admin"]}
                    element={<ExampleComponent />}
                  />
                }
              />
              <Route
                path="example/:id"
                element={
                  <ProtectedRoute
                    roles={["Admin"]}
                    element={<ExampleComponent />}
                  />
                }
              />
              <Route path="*" element={<Navigate to="/" replace />} />
            </Route>
          </Routes>
        </BrowserRouter>
      )}
    </UserContext.Provider>
  );
}
