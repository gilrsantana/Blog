# Sequence Diagrams

This document contains sequence diagrams illustrating the flow of messages between layers for key operations.

## 1. Create Post Flow (CQRS & Result Pattern)

```mermaid
sequenceDiagram
    autonumber
    actor Admin as Administrator
    participant PC as PostsController (Presentation)
    participant C as CreatePostCommand (Application)
    participant H as CreatePostCommandHandler (Application)
    participant PE as Post Entity (Domain)
    participant R as IPostRepository (Infrastructure)
    participant DB as MySQL Database

    Admin ->> PC: POST /api/posts { title, slug, summary, content, tags, coverImage }
    PC ->> C: Instantiates Command
    PC ->> H: HandleAsync(command)
    
    H ->> PE: Post.Create(title, slug, summary, content, tags, coverImage, authorId)
    note over PE: Validates fields & calculates ReadingTime (heuristic)
    alt Validation fails
        PE -->> H: Result.Failure(ValidationErrors)
        H -->> PC: Result.Failure(ValidationErrors)
        PC -->> Admin: HTTP 400 Bad Request (ProblemDetails)
    else Validation succeeds
        PE -->> H: Result.Success(PostInstance)
        H ->> R: AddAsync(PostInstance)
        R ->> DB: INSERT INTO Posts ...
        DB -->> R: Success
        R -->> H: Success
        H -->> PC: Result.Success(PostId)
        PC -->> Admin: HTTP 201 Created { id }
    end
```

---

## 2. Authentication Flow (ASP.NET Core Identity & JWT)

```mermaid
sequenceDiagram
    autonumber
    actor Client as Reader/Admin Client
    participant AC as AuthController (Presentation)
    participant H as LoginCommandHandler (Application)
    participant UM as UserManager (Infrastructure/Identity)
    participant DB as MySQL Database

    Client ->> AC: POST /api/auth/login { email, password }
    AC ->> H: HandleAsync(LoginCommand)
    
    H ->> UM: FindByEmailAsync(email)
    UM ->> DB: SELECT FROM Accounts ...
    DB -->> UM: Account Record
    UM -->> H: Account Instance

    alt Account not found
        H -->> AC: Result.Failure(AccountErrors.NotFound)
        AC -->> Client: HTTP 404 Not Found
    else Account found
        H ->> UM: CheckPasswordAsync(AccountInstance, password)
        UM -->> H: IsValid (True/False)
        alt Password invalid
            H -->> AC: Result.Failure(AccountErrors.InvalidCredentials)
            AC -->> Client: HTTP 401 Unauthorized
        else Password valid
            H ->> H: GenerateJwtToken(AccountInstance)
            H -->> AC: Result.Success(JwtToken)
            AC -->> Client: HTTP 200 OK { token }
        end
    end
```

---

## 3. Refresh Token Flow (Token Rotation & Session Extension)

```mermaid
sequenceDiagram
    autonumber
    actor Client as Reader/Admin Client
    participant AC as AuthController (Presentation)
    participant H as RefreshTokenCommandHandler (Application)
    participant TS as ITokenService (Infrastructure/Security)
    participant UM as UserManager (Infrastructure/Identity)
    participant DB as MySQL Database

    Client ->> AC: POST /api/auth/refresh { accessToken, refreshToken }
    AC ->> H: HandleAsync(RefreshTokenCommand)
    
    H ->> TS: GetPrincipalFromExpiredToken(accessToken)
    TS -->> H: ClaimsPrincipal (or Failure)
    
    alt Invalid access token / signature
        H -->> AC: Result.Failure(TokenErrors.InvalidToken)
        AC -->> Client: HTTP 400 Bad Request
    else Valid access token principal
        H ->> UM: FindByIdAsync(accountId)
        UM ->> DB: SELECT FROM Accounts WHERE Id = accountId
        DB -->> UM: Account Record
        UM -->> H: Account Instance

        alt Account not found or inactive
            H -->> AC: Result.Failure(AccountErrors.NotFound)
            AC -->> Client: HTTP 401 Unauthorized
        else Account found
            H ->> H: ValidateRefreshToken(AccountInstance, refreshToken)
            alt Refresh token invalid or expired (Account.RefreshTokenExpiryTime < DateTime.UtcNow)
                H -->> AC: Result.Failure(TokenErrors.ExpiredToken)
                AC -->> Client: HTTP 401 Unauthorized
            else Refresh token valid
                H ->> TS: GenerateAccessToken(AccountInstance)
                TS -->> H: NewAccessToken
                H ->> TS: GenerateRefreshToken()
                TS -->> H: NewRefreshToken
                
                H ->> H: AccountInstance.UpdateRefreshToken(NewRefreshToken, ExpiryTime)
                H ->> UM: UpdateAsync(AccountInstance)
                UM ->> DB: UPDATE Accounts SET RefreshToken, RefreshTokenExpiryTime WHERE Id = accountId ...
                DB -->> UM: Success
                UM -->> H: Success
                
                H -->> AC: Result.Success(NewAccessToken, NewRefreshToken)
                AC -->> Client: HTTP 200 OK { accessToken: NewAccessToken, refreshToken: NewRefreshToken }
            end
        end
    end
```

