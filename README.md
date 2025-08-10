# AuthService


Docker:

git clone link -b branchName

Из корня проекта

docker build -t auth_service .

docker run -p 5000:80 --name auth-container auth_service

docker rm auth-container

docker rmi auth_service

Доступно по http://localhost:5000

Swagger http://localhost:5000/swagger

Остальное сами найдете.
