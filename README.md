# ASP.NET_social_platform

# English:
Social Platform is a web application developed in ASP.NET Core MVC and C#, using Entity Framework Core for data persistence and ASP.NET Identity for authentication and role management. The project simulates the core features of a modern social network, including public and private profiles, a unidirectional follow system and discussion groups. Additionally, the platform integrates an AI companion for the automatic moderation of inappropriate content.

### User Management and Profiles
 * **User types:** the platform supports three distinct roles - unregistered visitor, registered user, and administrator.
 * **Profile creation and editing:** registered users can configure their profile (name, description, profile picture) and set its visibility (public or private).
 * **Advanced search:** users can be searched by full or partial name; profiles appear in results even when searching for name fragments.
 * **Profile viewing:**
    - **public profile:** full content is visible (name, description, picture, posts).
    - **private profile:** only basic information (name, description, picture) is displayed for those who do not follow the account.

### Social Interaction (Follow & Feed)
  * **Follow system:** unidirectional relationships (Instagram style).
  * **For public accounts:** following is instant.
  * **For private accounts:** the request receives a "Pending" status until accepted or refused.
  * **Personalized feed:** each user has a news feed displaying posts from followed users, ordered descending by date.
  * **Reactions:** users can appreciate posts with various reactions but cannot react twice to the same post.

### Posts and Media Content
  * **Content types:** support for posts containing text, images, and videos.
  * **Content management:** users can add, edit and delete their own posts and comments.
  * **Comments:** the ability to comment on other users' posts.
  * **Display:** content (posts and comments) is displayed in descending order by date.

### Saved Posts
  * **Functionality:** registered users can save posts for later viewing using a "Save/Unsave" toggle button on each post.
  * **Access:** a dedicated section is available within the user's profile.
  * **Privacy:** the list of saved posts is strictly private; users cannot view what others have saved.  

### Groups and Communities
  * **Create groups:** users can create groups, automatically becoming moderators. Groups require a name and description.
  * **Members:** users can request to enter groups ("Join"), requiring acceptance by the moderator.
  * **Communication:** members can send messages within the group and edit/delete their own messages.
  * **Group moderation:** the moderator can remove members and delete the created group.  

### Group Discovery & Search
  * **Search capability:** users can search for existing groups by full or partial name and description.
  * **Visibility:** the search function is available to all users, even if they are not members of those groups.
  * **Results:** displays the group name, description, and the moderator's name.
  * **Performance:** search results are paginated to ensure optimal loading times.
  * **Algorithm:** the search is case-insensitive (e.g., "coding" matches "Coding").

### AI Integration
  * **Content filtering:** An AI companion automatically checks texts before posts or comments are published.
  * **Detection:** Identifies inappropriate language, insults, or hate speech.
  * **Blocking:** If inappropriate content is detected, the system blocks publication and displays a friendly error message suggesting reformulation.

### Administration
 * **Control:** The administrator has moderation rights over the platform.
 * **Content cleaning:** Can delete any element considered inappropriate, including comments, messages, users, or entire groups, but cannot edit their content.

### The team that developed this project:
 * **Bâcă Ionuț Adelin**
 * **Popescu Iulia Maria**
 * **Trifan Antonia Mirabella**
      
# Română:
Social Platform este o aplicație web dezvoltată în ASP.NET Core MVC și C#, utilizând Entity Framework Core pentru persistența datelor și ASP.NET Identity pentru gestionarea autentificării și a rolurilor. Proiectul simulează funcționalitățile de bază ale unei rețele de socializare moderne, incluzând profiluri publice și private, un sistem de urmărire (follow) unidirecțional și grupuri de discuții. De asemenea, platforma integrează un companion AI pentru moderarea automată a conținutului neadecvat.

### Gestionarea Utilizatorilor și Profiluri
  * **Tipuri de utilizatori:** platforma suportă trei roluri distincte - vizitator neînregistrat, utilizator înregistrat și administrator.
  * **Creare și editare profil:** utilizatorii înregistrați își pot configura profilul (nume, descriere, poză de profil) și pot seta vizibilitatea acestuia (public sau privat).
  * **Căutare avansată:** posibilitatea de a căuta utilizatori după numele complet sau parțial; profilurile sunt afișate în rezultate chiar dacă se caută doar fragmente din nume.
  * **Vizualizare profil:**
      - **profil public:** conținut complet vizibil (nume, descriere, poză, postări).
      - **profil privat:** sunt afișate doar informațiile de bază (nume, descriere, poză) pentru cei care nu urmăresc contul.

### Interacțiune Socială (Follow & Feed)
 * **Sistem de urmărire (Follow):** relații unidirecționale (stil Instagram).
 * **Pentru conturile publice:** urmărirea este instantanee.
 * **Pentru conturile private:** cererea primește statusul "Pending" până la acceptare sau refuz.
 * **Feed personalizat:** fiecare utilizator are un flux de noutăți care afișează postările persoanelor urmărite, ordonate descrescător după dată.
 * **Reacții:** utilizatorii pot aprecia postările cu diverse reacții, dar nu pot reacționa de două ori la aceeași postare.

### Postări și Conținut Media
 * **Tipuri de conținut:** suport pentru postări ce conțin text, imagini și videoclipuri.
 * **Gestionare conținut:** utilizatorii pot adăuga, edita și șterge propriile postări și comentarii.
 * **Comentarii:** posibilitatea de a comenta la postările altor utilizatori.
 * **Afișare:** conținutul (postări și comentarii) este afișat în ordine descrescătoare după dată.

### Postări Salvate
 * **Funcționalitate:** utilizatorii înregistrați pot marca postări pentru a le vizualiza ulterior, folosind un buton de tip "Save/Unsave".
 * **Acces:** postările salvate sunt accesibile într-o secțiune dedicată din profilul utilizatorului.
 * **Confidențialitate:** lista este privată; utilizatorii nu pot vedea postările salvate de alte persoane.
  
### Grupuri și Comunități
 * **Creare grupuri:** utilizatorii pot crea grupuri, devenind automat moderatori. Grupurile necesită denumire și descriere.
 * **Membri:** utilizatorii pot cere să intre în grupuri ("Join"), fiind necesară acceptarea de către moderator.
 * **Comunicare:** membrii pot trimite mesaje în grup și își pot edita/șterge propriile mesaje.
 * **Moderare grup:** moderatorul poate elimina membri și poate șterge grupul creat.

### Căutare și Descoperire Grupuri
 * **Capabilitate:** utilizatorii pot căuta grupuri existente după denumire sau descriere (căutare după text complet sau parțial).
 * **Vizibilitate:** funcționalitatea este disponibilă tuturor utilizatorilor, indiferent dacă sunt sau nu membri ai grupurilor respective.
 * **Rezultate:** lista returnată include denumirea grupului, descrierea și numele moderatorului.
 * **Performanță:** rezultatele căutării sunt afișate paginat pentru a evita încărcarea excesivă a paginii.
 * **Algoritm:** căutarea este case-insensitive (ignoră diferențele dintre majuscule și minuscule).  

### Integrare AI 
 * **Filtrare conținut:** un companion AI verifică automat textele înainte de publicarea postărilor sau comentariilor.
 * **Detecție:** identifică limbajul nepotrivit, insultele sau discursul instigator la ură.
 * **Blocare:** dacă este detectat conținut neadecvat, sistemul blochează publicarea și afișează un mesaj de eroare prietenos care sugerează reformularea.

### Administrare
 * **Control:** administratorul are drepturi de moderare asupra platformei.
 * **Curățare conținut:** poate șterge orice element considerat neadecvat, inclusiv comentarii, mesaje, utilizatori sau grupuri întregi, dar nu poate edita conținutul acestora.

### Echipa care a dezvoltat acest proiect:
 * **Bâcă Ionuț Adelin**
 * **Popescu Iulia Maria**
 * **Trifan Antonia Mirabella**

