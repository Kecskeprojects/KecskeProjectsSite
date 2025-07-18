import { LineSpinner } from "ldrs/react";
import "ldrs/react/LineSpinner.css";
import { useEffect, useState } from "react";
import { BrowserRouter, Route, Routes } from "react-router-dom";
import "./App.css";
import { UserContext } from "./Contexts";
import ExampleComponent from "./ExampleComponent";
import Home from "./Home";
import Layout from "./Layout";
import Login from "./Login";
import UserData from "./Models/UserData";
import AccountService from "./Services/AccountService";

export default function App() {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!user) {
      AccountService.GetLoggedInUser()
        .then((data) => {
          setUser(new UserData(data));
          setLoading(false);
        })
        .catch((error) => console.log(error));
    }
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
      {!user ? (
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
