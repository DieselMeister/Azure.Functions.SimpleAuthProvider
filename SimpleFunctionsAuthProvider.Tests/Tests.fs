module Tests

open System
open Azure.Functions.SimpleAuth
open Xunit
open Swensen.Unquote
open System
open Azure.Functions.SimpleAuth.Domain


module Authentication =

    [<Fact>]
    let ``authentication should return None if user doesn't exisits`` () =
        async {
            let getUser app username = async { return None }
            let storeToken token = async { return None }
            let! result = Domain.Authentication.authenticate getUser storeToken "app" "user" "password"
            test <@ result = None @>            
        }

    [<Fact>]
    let ``Authentication should return None if store token doesn't work`` () =
        async {
            let (user:Domain.User) = { AppName="app"; UserName="user"; PasswordHash=""; Salt=""; Groups=""}
            let getUser app username = async { return Some user }
            let storeToken token = async { return None }
            let! result = Domain.Authentication.authenticate getUser storeToken "app" "user" "password"
            test <@ result = None @>
        }

    [<Fact>]
    let ``Authentication should return a token when the username and pasword is correct`` () =
        async {
            let pwhash = "nml3q3dR8Se6S3NuPGaDF2EL7SpeZcFJ7XrKg8xOsg4="
            let salt = "StCBeGkDw7wamQhG3zfmZw=="
            let (user:Domain.User) = { AppName="app"; UserName="user"; PasswordHash=pwhash; Salt=salt; Groups=""}
            let getUser app username = async { return Some user }
            let storeToken token = async { return Some "irgendwas" }
            let! result = Domain.Authentication.authenticate getUser storeToken "app" "user" "secret"
            
            match result with
            | None -> 
                test <@ result <> None @>
            | Some token ->
                test <@ token.Token.Length  > 0 @>
        }

    [<Fact>]
    let ``Authentication should return None when the password is incorrect`` () =
        async {
            // pw: secret
            let pwhash = "nml3q3dR8Se6S3NuPGaDF2EL7SpeZcFJ7XrKg8xOsg4="
            let salt = "StCBeGkDw7wamQhG3zfmZw=="
            let (user:Domain.User) = { AppName="app"; UserName="user"; PasswordHash=pwhash; Salt=salt; Groups=""}
            let getUser app username = async { return Some user }
            let storeToken token = async { return Some "irgendwas" }
            let! result = Domain.Authentication.authenticate getUser storeToken "app" "user" "password"
            test <@ result = None @>
        }

    
        
