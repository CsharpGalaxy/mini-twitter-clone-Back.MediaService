# مرحله base برای runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# مرحله build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# فقط فایل csproj پروژه را کپی می‌کنیم و restore می‌زنیم
COPY ["MiniTwitter.MediaService/MiniTwitter.MediaService.csproj", "MiniTwitter.MediaService/"]
RUN dotnet restore "MiniTwitter.MediaService/MiniTwitter.MediaService.csproj"

# حالا بقیه سورس‌ها را کپی می‌کنیم
COPY . .

# می‌ریم داخل فولدر پروژه و publish می‌کنیم
WORKDIR "/src/MiniTwitter.MediaService"
RUN dotnet publish "MiniTwitter.MediaService.csproj" -c Release -o /app/publish /p:UseAppHost=false

# مرحله نهایی
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MiniTwitter.MediaService.dll"]
