export default class BaseService {
  static BackendRoute: string | undefined = process.env.REACT_APP_BACKEND_URL;
  static GetUserStateEndpoint: string = "/Account/GetLoggedInUser";

  static async Get(route: string, additionalHeaders?: Headers): Promise<any> {
    return BaseService.BaseFetch("GET", route, undefined, additionalHeaders);
  }

  static async Post(
    route: string,
    body?: FormData,
    additionalHeaders?: Headers
  ): Promise<any> {
    return BaseService.BaseFetch("POST", route, body, additionalHeaders);
  }

  static async Put(
    route: string,
    body?: FormData,
    additionalHeaders?: Headers
  ): Promise<any> {
    return BaseService.BaseFetch("PUT", route, body, additionalHeaders);
  }

  static async Delete(
    route: string,
    body?: FormData,
    additionalHeaders?: Headers
  ): Promise<any> {
    return BaseService.BaseFetch("DELETE", route, body, additionalHeaders);
  }

  static async BaseFetch(
    method: string,
    route: string,
    body?: FormData,
    additionalHeaders?: Headers
  ): Promise<any> {
    if (!route.startsWith("/") && !route.startsWith("\\")) {
      route = "/" + route;
    }

    const response = await fetch(`${BaseService.BackendRoute}${route}`, {
      method: method,
      credentials: "include",
      headers: additionalHeaders,
      body: body,
    });

    const responseBody = await response.json();

    if (process.env.NODE_ENV === "development") {
      console.log(response.status);
      console.log(responseBody);
    }

    if (response.status !== 200) {
      BaseService.ErrorHandling(route, response.status, responseBody);
    }

    return responseBody;
  }

  static ErrorHandling(route: string, status: number, body: any) {
    if (body.error) {
      console.log(`Status Code: ${status}\nError: ${body.error}`);
    }

    if (status === 401 && !route.startsWith(BaseService.GetUserStateEndpoint)) {
      window.location.reload();
    }
  }
}
