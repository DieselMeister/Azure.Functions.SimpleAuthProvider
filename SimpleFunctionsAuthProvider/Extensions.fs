namespace Azure.Functions.SimpleAuth

open System.Security.Claims
open System.Collections
open System.Net
open Microsoft.AspNetCore.Http
open FSharp.Json


module Extensions =

    
    module FSharp =

        let userInfoToClaims (user:Domain.UserInfo) =
            let claims = 
                user.Groups
                |> List.map (fun i -> Claim("Role",i))

            let identity = ClaimsIdentity(claims @ [ Claim ("Name",user.UserName) ] ,"Token","Name", "Role")
            ClaimsPrincipal(identity)


        /// get claim principal from token directly from the azure storage (F# option)
        let getUserClaimFromToken token tableClient =
            async {
                let! user = Services.getUserInfo token tableClient
                return 
                    user 
                    |> Option.map (userInfoToClaims)
            }

        let getCurrentBaseUrl (req:HttpRequest) =
            sprintf "%s://%s/" req.Scheme req.Host.Value


        /// get claim principal from token from the authentication endpoint (F# option)
        let getUserClaimFromTokenEndpoint (token:string) (userInfoEndpointAddress:string) =
            async {
                use client = new System.Net.Http.HttpClient()
                client.DefaultRequestHeaders.Add("Authorization", token)
                let! result = client.GetAsync(userInfoEndpointAddress) |> Async.AwaitTask
                if result.StatusCode = HttpStatusCode.OK then
                    let! content = result.Content.ReadAsStringAsync() |> Async.AwaitTask
                    let config = JsonConfig.create(jsonFieldNaming = Json.lowerCamelCase)
                    let userInfo = FSharp.Json.Json.deserializeEx<Domain.UserInfo> config content 
                    return Some (userInfo |> userInfoToClaims)
                else
                    return None
            }


    module CSharp =

        // for our C# Users
        type IUserInfoExtension =
            abstract member GetUserClaimFromToken: string -> System.Threading.Tasks.Task<ClaimsPrincipal>

        type UserInfoExtensionAzureStore(tableClient) =
            interface IUserInfoExtension with
                member this.GetUserClaimFromToken(token) =
                    async {
                        let! claims = FSharp.getUserClaimFromToken token tableClient
                        match claims with
                        | None -> return null
                        | Some claims -> return claims
                    } |> Async.StartAsTask
                
        type UserInfoExtensionHttpEndpoint(userInfoEndpointAddress) =
            interface IUserInfoExtension with
                member this.GetUserClaimFromToken(token) = 
                    async {
                        let! claims = FSharp.getUserClaimFromTokenEndpoint token userInfoEndpointAddress
                        match claims with
                        | None -> return null
                        | Some claims -> return claims
                    } |> Async.StartAsTask
                
        


