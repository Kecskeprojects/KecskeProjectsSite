import { LineSpinner } from "ldrs/react";
import "ldrs/react/LineSpinner.css";
import { useEffect, useState } from "react";
import { BrowserRouter, Route, Routes } from "react-router-dom";
import { UserContext } from "./components/Contexts";
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
  }, [user]);

  if (loading) {
    //https://uiball.com/ldrs/
    return (
      <div style={{ margin: "200px auto auto auto", width: "min-content" }}>
        <LineSpinner size={500} color="rgb(0,0,0)" speed={1} stroke={20} />
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
              <Route path="example" element={<ExampleComponent />} />
            </Route>
          </Routes>
        </BrowserRouter>
      )}
    </UserContext.Provider>
  );
}
