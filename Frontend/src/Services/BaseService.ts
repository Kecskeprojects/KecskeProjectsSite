export default class BaseService {
  static BackendRoute: string | undefined = import.meta.env.VITE_BACKEND_URL;
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

    if (import.meta.env.MODE !== "production") {
      console.log(responseBody);
    }

    if (response.status !== 200) {
      BaseService.ErrorHandling(route, response.status, responseBody);
    }

    return responseBody;
  }

  static ErrorHandling(route: string, status: number, body: any) {
    //Todo: Proper user friendly error handling using floating popups or something
    if (body.error) {
      console.log(`Status Code: ${status}\nError: ${body.error}`);
    }

    if (status === 401 && !route.startsWith(BaseService.GetUserStateEndpoint)) {
      window.location.reload();
    }
  }
}
