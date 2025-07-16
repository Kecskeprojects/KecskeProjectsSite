import { Link } from "react-router-dom";

export default function Home() {
  return (
    <div>
      Home Page
      <div>
        <Link to="example">Example Page</Link>
      </div>
    </div>
  );
}
