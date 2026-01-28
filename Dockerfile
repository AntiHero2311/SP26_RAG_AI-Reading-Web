# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy toàn bộ solution để đảm bảo nhận diện đủ các project con
COPY . .

# Restore dependencies cho project chính (Render sẽ tự tìm các project phụ liên quan)
RUN dotnet restore "SP26_BE/RAG_AI_Reading/RAG_AI_Reading.csproj"

# Build project chính
RUN dotnet publish "SP26_BE/RAG_AI_Reading/RAG_AI_Reading.csproj" -c Release -o /app/publish

# Stage 2: Run
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Cấu hình Port cho Render
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

# Chú ý: Thay đúng tên file .dll của project API (thường trùng tên folder project)
ENTRYPOINT ["dotnet", "RAG_AI_Reading.dll"]