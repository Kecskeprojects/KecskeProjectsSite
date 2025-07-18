export default class BaseService {
  static async Get(route, additionalHeaders = {}) {
    return this.BaseFetch("GET", route, null, additionalHeaders);
  }

  static async Post(route, body = {}, additionalHeaders = {}) {
    return this.BaseFetch("POST", route, body, additionalHeaders);
  }

  static async Put(route, body = {}, additionalHeaders = {}) {
    return this.BaseFetch("PUT", route, body, additionalHeaders);
  }

  static async Delete(route, body = {}, additionalHeaders = {}) {
    return this.BaseFetch("DELETE", route, body, additionalHeaders);
  }

  static async BaseFetch(method, route, body, additionalHeaders) {
    if (!route.startsWith("/") && !route.startsWith("\\")) {
      route = "/" + route;
    }

    return fetch(`${process.env.REACT_APP_BACKEND_URL}${route}`, {
      method: method,
      credentials: "include",
      headers: new Headers({
        ...additionalHeaders,
      }),
      body: body,
    })
      .then((res) => {
        if (process.env.NODE_ENV === "development") {
          console.log(res);
        }

        if (res.status === 401) {
          throw Error("You are not logged in!");
        }
        if (res.status === 403) {
          throw Error("You cannot access this data!");
        }
        if (res.status !== 200) {
          throw Error("Internal Error!");
        }
        return res;
      })
      .then((data) => data.json());
  }
}
