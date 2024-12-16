import socket
import threading
import json

# Configuration du serveur
HOST = '0.0.0.0'  # Écouter sur toutes les interfaces réseau
PORT = 5000  # Port utilisé par le serveur

# Liste des connexions
clients = []

def handle_client(client_socket, address):
    """
    Gère les connexions clients (téléphone ou Unity).
    """
    print(f"Connexion établie avec : {address}")
    buffer = ""

    while True:
        try:
            # Recevoir des données du client
            data = client_socket.recv(1024).decode('utf-8')
            if not data:
                print(f"Connexion terminée avec : {address}")
                clients.remove(client_socket)
                break

            # Ajouter les données reçues au tampon
            buffer += data

            # Traiter les messages complets dans le tampon
            while True:
                # Chercher la fin du message (par exemple, un '\n' ou un caractère de fin)
                message_end_index = buffer.find('}\n')  # Changez le délimiteur selon votre format
                if message_end_index == -1:
                    # Pas de message complet, sortir de la boucle
                    break
                
                # Extraire le message complet et le traiter
                message = buffer[:message_end_index + 2]  # Inclure '}\n'
                buffer = buffer[message_end_index + 2:]  # Garder le reste dans le tampon
                
                # Diffuser le message complet
                print(f"Données reçues de {address} : {message}")
                broadcast_to_unity(message)
        
        except Exception as e:
            print(f"Erreur avec le client {address} : {e}")
            clients.remove(client_socket)
            break

    client_socket.close()


def broadcast_to_unity(data):
    """
    Envoie les données reçues du téléphone à Unity.
    """
    print(f"Diffusion des données à Unity : {data}")  # Ajoutez ceci
    for client in clients:
        try:
            client.sendall(data.encode('utf-8'))
        except Exception as e:
            print(f"Erreur d'envoi à Unity : {e}")
            clients.remove(client)

def main():
    """
    Fonction principale pour démarrer le serveur.
    """
    # Créer le socket serveur
    server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    server_socket.bind((HOST, PORT))
    server_socket.listen(5)
    print(f"Serveur démarré sur {HOST}:{PORT}")

    try:
        while True:
            # Accepter une nouvelle connexion
            client_socket, address = server_socket.accept()
            clients.append(client_socket)
            print(f"Nouvelle connexion depuis : {address}")

            # Lancer un thread pour gérer ce client
            client_thread = threading.Thread(target=handle_client, args=(client_socket, address))
            client_thread.start()

    except KeyboardInterrupt:
        print("\nArrêt du serveur.")
    finally:
        server_socket.close()

if __name__ == "__main__":
    main()