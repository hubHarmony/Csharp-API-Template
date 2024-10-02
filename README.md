
# Template d'API .NET avec Authentification JWT 🚀

## Présentation

Cette API simple est construite avec ASP.NET Core et permet de gérer diverses routes et protocoles. Elle inclut un système d'authentification basé sur les JSON Web Tokens (JWT) pour sécuriser les requêtes. 🔐

## Fonctionnalités

- **API RESTful** : Prend en charge les méthodes HTTP standards (GET, POST, PUT, DELETE). 🌐
- **Authentification JWT** : Sécurisez vos points de terminaison API avec des jetons JWT. 🛡️
- **Gestion des Routes** : Facilité de gestion et de définition des routes pour votre application. 📍
- **Support Multi-Protocole** : Gère plusieurs protocoles pour la flexibilité des requêtes. 🔄

## Installation

### Prérequis

- [.NET SDK](https://dotnet.microsoft.com/download) (version 8.0 ou supérieure) 💻
- [Visual Studio Code](https://code.visualstudio.com/) ou [JetBrains Rider](https://www.jetbrains.com/rider/) pour le développement 🛠️
- [MySQL Server](https://www.mysql.com/) pour la base de données 💾

### Forker le projet

1. **Forker le dépôt** :
   - Allez sur la page du projet sur GitHub et cliquez sur le bouton **Fork** en haut à droite pour créer votre propre copie du projet.

2. **Cloner votre fork** :
   - Ouvrez votre terminal et exécutez la commande suivante :
     ```bash
     git clone https://github.com/votre-utilisateur/votre-repo.git
     cd votre-repo
     ```

3. **Installer les dépendances** :
   ```bash
   dotnet restore
   ```

### Configurer votre base de données MySQL

1. **Créer une base de données MySQL** :
   - Utilisez un outil comme MySQL Workbench ou un terminal pour créer une nouvelle base de données.

2. **Modifier la chaîne de connexion** :
   - Ouvrez le fichier `appsettings.json` dans votre projet et modifiez la section `ConnectionStrings` pour y inclure vos informations de connexion :
     ```json
     {
       "ConnectionStrings": {
         "DefaultConnection": "Server=localhost;Database=nom_de_votre_base;User=utilisateur;Password=motdepasse;"
       }
     }
     ```

## Utilisation

### Lancer l'API

Pour démarrer l'application, exécutez la commande suivante :

```bash
dotnet run
```

L'API sera accessible à l'adresse `http://localhost:5000`.

### Authentification JWT

1. **Inscription d'un utilisateur** :
   - Endpoint : `POST /api/signup`
   - Corps de la requête :
     ```json
     {
       "username": "votreNom",
       "password": "votreMotDePasse"
     }
     ```

2. **Connexion d'un utilisateur** :
   - Endpoint : `POST /api/login`
   - Corps de la requête :
     ```json
     {
       "username": "votreNom",
       "password": "votreMotDePasse"
     }
     ```
   - Réponse :
     ```json
     {
       "token": "votreJWT"
     }
     ```

3. **Accéder aux données sécurisées** :
   - Endpoint : `GET /api/data`
   - En-tête de la requête :
     ```
     Authorization: SST votreJWT
     ```
     SST est ici l'acronyme de Super Secure Token, un JavaWebToken généré automatiquement lors du login et dont la signature sera vérifiée à chaque requête nécéssitant un compte utilisateur. 

## Exemples de Requêtes

### Inscription d'un utilisateur

```bash
curl -X POST http://localhost:5000/api/signup \
-H "Content-Type: application/json" \
-d '{"username": "testuser", "password": "testpass"}'
```

### Connexion d'un utilisateur

```bash
curl -X POST http://localhost:5000/api/login \
-H "Content-Type: application/json" \
-d '{"username": "testuser", "password": "testpass"}'
```

### Accéder aux données sécurisées

```bash
curl -X GET http://localhost:5000/api/data \
-H "Authorization: Bearer votreJWT"
```

## Conclusion 🎉

Cette API simple vous permet de gérer efficacement des requêtes tout en garantissant la sécurité grâce à l'authentification JWT. N'hésitez pas à personnaliser et à étendre cette API selon vos besoins !

## Aide et Support

Pour toute question ou problème, ouvrez une issue sur le [dépôt GitHub](https://github.com/hubHarmony/Csharp-API-Template/issues/new). 📬
