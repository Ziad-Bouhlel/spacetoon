
Pour lancer le serveur :

& "venv\Scripts\python.exe" "main.py"


Pour voir si y a d'autres serveur ouvert sur le port :

netstat -ano | findstr :5000

taskkill /PID Numero_port /F
