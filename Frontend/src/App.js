import { BrowserRouter, Route, Routes } from "react-router-dom";
import "./App.css";
import ExampleComponent from "./ExampleComponent";
import Home from "./Home";
import Layout from "./Layout";

function App() {
  console.log(process.env.NODE_ENV);
  console.log(process.env.REACT_APP_BACKEND_URL);
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Layout />}>
          <Route index element={<Home />} />
          <Route path="example" element={<ExampleComponent />} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
}

export default App;
