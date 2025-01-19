import socket
import threading

# Configuration du serveur
HOST = '0.0.0.0'  # Écouter sur toutes les interfaces réseau
PORT = 5000  # Port utilisé par le serveur

# Liste des connexions et identités
clients = {}  # Associe chaque socket client à une identité
server_running = True  # Flag pour contrôler l'exécution du serveur

clients_by_name = {}

def handle_client(client_socket, address):
    """
    Gère les connexions clients (hockeyJoueur, puzzleEcran, etc.).
    """
    print(f"Connexion établie avec : {address}")

    try:
        # Recevoir la première donnée pour identifier le client
        print("Envoi de la demande 'IDENTIFY' au client...")
        client_socket.sendall("IDENTIFY\n".encode('utf-8'))  # Demande d'identification
        print("Envoi de la demande 'IDENTIFY' au client...")
        identity = client_socket.recv(1024).decode('utf-8')  # Réception de l'identité
        print(f"Identité reçue : {identity}")
        if not identity:
            raise Exception("Aucune identité reçue.")

        print(f"Client identifié : {identity} depuis {address}")
        clients[client_socket] = {"address": address, "identity": identity}
        clients_by_name[identity] = client_socket
        while True:
            # Recevoir des données du client
            data = client_socket.recv(1024)
            if not data:
                print(f"Connexion terminée avec : {address}")
                break

            # Décoder les données reçues
            message = data.decode('utf-8')
            print(f"Données reçues de {identity} ({address}) : {message}")

            # Appeler une méthode spécifique en fonction de l'identité du client
            handle_message(identity, message)

    except Exception as e:
        print(f"Erreur avec le client {address} : {e}")
    finally:
        # Supprimer le client de la liste et fermer la connexion
        if client_socket in clients:
            del clients[client_socket]
        if identity in clients_by_name:
            del clients_by_name[identity]
        client_socket.close()


def handle_message(identity, message):
    """
    Gère les messages reçus en fonction de l'identité du client.
    """
    #broadcast_to_unity(message)
    # Simule un switch/case pour Python
    if identity == "hockeyJoueur1":
        handle_hockey_joueur1(message)
    elif identity == "hockeyJoueur2":
        handle_hockey_joueur2(message)
    elif identity == "hockeyJeu":
        handle_hockey_jeu(message)
    elif identity == "menuDuJeu":
        handle_menu_du_jeu(message)
    elif identity == "puzzleEcran":
        handle_puzzle_ecran(message)
    elif  "DragAndDrop" in identity:
        handle_drag_and_drop(message)

    else:
        print(f"Identité inconnue : {identity}. Message ignoré.")


# Méthodes pour traiter les messages des différentes identités
def handle_hockey_joueur1(message):
    print(f"Traitement de hockeyJoueur1 : {message}")
    send_to_client("hockeyJeu", message)

def handle_hockey_joueur2(message):
    print(f"Traitement de hockeyJoueur2 : {message}")
    send_to_client("hockeyJeu", message)

def handle_hockey_jeu(message):
    if message.startswith("IDENTITY:hockeyJeu|GOAL:"):
        _, goal_message = message.split("|", 1)  # Sépare l'identité et la commande
        _, team = goal_message.split("GOAL:")  # Extrait l'équipe qui a marqué

        team = team.strip()
        print(f"Équipe ayant marqué : {team}")
        print("blue" == team)
        # Détermine quel joueur notifier
        player_identity = "hockeyJoueur1" if team == "blue" else "hockeyJoueur2"
        
        print(f"Envoi de vibration à {player_identity}")
        # Envoie la vibration au joueur concerné
        if player_identity in clients_by_name:
            try:
                client_socket = clients_by_name[player_identity]
                client_socket.sendall("VIBRATE\n".encode('utf-8'))  # Message de vibration
                print(f"Message de vibration envoyé à {player_identity}")
            except Exception as e:
                print(f"Erreur d'envoi à {player_identity} : {e}")
        else:
            print(f"Client {player_identity} introuvable.")
    else:
        print("Message non reconnu ou non pertinent.")



def handle_menu_du_jeu(message):
    if "puzzle" in message:
        send_to_client("puzzleEcran", message)

def handle_puzzle_ecran(message):
    print(f"Traitement de puzzleEcran : {message}")
    send_to_client("DragAndDropPrincipal", message)

def handle_drag_and_drop(message):
    print(f"Traitement de dragAndDrop : {message}")
    send_to_client("puzzleEcran", message)

def monitor_keyboard():
    """
    Surveille les entrées clavier pour arrêter le serveur.
    Tapez 'q' pour quitter.
    """
    global server_running
    while True:
        user_input = input()
        if user_input.lower() == 'q':
            print("Arrêt du serveur demandé.")
            server_running = False
            break

def broadcast_to_unity(data):
    """
    Envoie les données reçues du téléphone à Unity.
    """
    for client in clients:
        try:
            client.sendall(data.encode('utf-8'))
        except Exception as e:
            print(f"Erreur d'envoi à Unity : {e}")
            clients.remove(client)

def send_to_client(client_name, message):
    global clients_by_name  

    if client_name in clients_by_name:
        try:
            client_socket = clients_by_name[client_name]
            client_socket.sendall(message.encode('utf-8'))
            print(f"Message envoyé à {client_name} : {message}")
        except Exception as e:
            print(f"Erreur d'envoi à {client_name} : {e}")
    else:
        print(f"Client {client_name} introuvable.")
def main():
    """
    Fonction principale pour démarrer le serveur.
    """
    global server_running
    # Créer le socket serveur
    server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    server_socket.bind((HOST, PORT))
    server_socket.listen(5)
    print(f"Serveur démarré sur {HOST}:{PORT}")
    print("Appuyez sur 'q' pour arrêter le serveur.")

    # Lancer un thread pour surveiller les entrées clavier
    keyboard_thread = threading.Thread(target=monitor_keyboard, daemon=True)
    keyboard_thread.start()

    try:
        while server_running:
            # Accepter une nouvelle connexion
            server_socket.settimeout(1)  # Timeout pour vérifier régulièrement le flag
            try:
                client_socket, address = server_socket.accept()
                print(f"Nouvelle connexion depuis : {address}")

                # Lancer un thread pour gérer ce client
                client_thread = threading.Thread(target=handle_client, args=(client_socket, address))
                client_thread.start()
            except socket.timeout:
                continue
    except KeyboardInterrupt:
        print("\nArrêt du serveur via Ctrl+C.")
    finally:
        print("Fermeture du serveur...")
        server_socket.close()
        for client_socket in list(clients.keys()):
            client_socket.close()
        print("Serveur arrêté.")

if __name__ == "__main__":
    main()
