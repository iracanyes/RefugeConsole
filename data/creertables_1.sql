

---------------------------------------------------------------------------------------------------------------------------------
-- Colors : Table, index et contraintes
---------------------------------------------------------------------------------------------------------------------------------

CREATE TABLE IF NOT EXISTS public."Colors"(
    "Id" uuid not null,
    "Name" text not null,
    CONSTRAINT "PK_Color" PRIMARY KEY ("Id")

);
ALTER TABLE public."Colors" OWNER TO ipefa_iracanyes;

CREATE UNIQUE INDEX IF NOT EXISTS "IX_Colors_Name" ON public."Colors" USING btree ("Name");

INSERT INTO public."Colors" ("Id", "Name")
VALUES (gen_random_uuid(), 'chocolat'),
       (gen_random_uuid(), 'blanc'),
       (gen_random_uuid(), 'noir'),
       (gen_random_uuid(), 'crème'),
       (gen_random_uuid(), 'gris'),
       (gen_random_uuid(), 'roux'),
       (gen_random_uuid(), 'lilas');

---------------------------------------------------------------------------------------------------------------------------------
-- Animal : Table, index et contraintes
---------------------------------------------------------------------------------------------------------------------------------

--
-- Name: Animals; Type: TABLE; Schema: public; Owner: ipefa_iracanyes
--

CREATE TABLE IF NOT EXISTS public."Animals" (
    "Id" varchar(11) NOT NULL,
    "Name" text NOT NULL,
    "Type" text NOT NULL,
    "Gender" varchar(1) NOT NULL,
    "BirthDate" date,
    "DeathDate" date,
    "IsSterilized" boolean NOT NULL,
    "DateSterilization" date,
    "Particularity" text NOT NULL,
    "Description" text NOT NULL,
    CONSTRAINT "PK_Animals" PRIMARY KEY ("Id"),
    -- Contrainte => identifiant: format valide yymmdd99999 (11 caractères)
    CONSTRAINT "Check_Animals_Animal_Id" CHECK ( "Id" ~* '^(\d{2})(0[1-9]|1[0-2])(0[1-9]|[1-2]\d|3[0-1])(\d{5})$' ),
    -- Contrainte => type : {'chat', 'chien'}
    CONSTRAINT "Check_Animals_Type" CHECK ( "Type" in ('chat', 'chien') ),
    -- Contrainte => sexe : {'M', 'F'}
    CONSTRAINT "Check_Animals_Gender" CHECK ( "Gender" IN ('M','F') ),
    -- Contrainte => Date de naissance doit être supérieur ou égal à la date courante
    CONSTRAINT "Check_Animals_BirthDate" CHECK ( "BirthDate" <= CURRENT_DATE ),
    -- Contrainte => Date de décès d’un animal est soit nulle ou soit supérieure à sa date de naissance
    CONSTRAINT "Check_Animals_DeathDate" CHECK ( "DeathDate" IS NULL OR ("DeathDate" > "BirthDate") ),
    -- Contrainte => Si l’animal n’est pas stérilisé alors la date de stérilisation est nulle
    CONSTRAINT "Check_Animals_IsSterilized" CHECK ( "IsSterilized" IS FALSE OR "DateSterilization" IS NOT NULL),
    -- Contrainte => La date de stérilisation est soit nulle ou soit supérieure à sa date de naissance
    CONSTRAINT "Check_Animals_DateSterilization" CHECK ( "DateSterilization" IS NULL OR ("DateSterilization" > "BirthDate") )

);


ALTER TABLE public."Animals" OWNER TO ipefa_iracanyes;

--
-- Index sur la propriété "Name" de la table Animal
--
CREATE INDEX IF NOT EXISTS  "IX_Animals_Name" ON public."Animals" USING btree ("Name");



---------------------------------------------------------------------------------------------------------------------------------
-- AnimalColors : Table, index et contraintes
---------------------------------------------------------------------------------------------------------------------------------

CREATE TABLE IF NOT EXISTS public."AnimalColors"(
    "Id" uuid not null,
    "AnimalId" varchar(11) not null,
    "ColorId" uuid not null,
    CONSTRAINT "PK_AnimalColors" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_AnimalColors_Color_AnimalId" FOREIGN KEY ("AnimalId") REFERENCES public."Animals"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_AnimalColors_Color_ColorId" FOREIGN KEY ("ColorId") REFERENCES public."Colors"("Id") ON DELETE CASCADE

);

ALTER TABLE public."AnimalColors" OWNER TO ipefa_iracanyes;

CREATE INDEX IF NOT EXISTS "IX_AnimalColors_AnimalId" ON public."AnimalColors" USING btree ("AnimalId");

CREATE INDEX IF NOT EXISTS "IX_AnimalColors_AnimalId" ON public."AnimalColors" USING btree ("ColorId");

---------------------------------------------------------------------------------------------------------------------------------
-- Adresse : Table, index et contraintes
---------------------------------------------------------------------------------------------------------------------------------

-- Name: Addresses; Type: TABLE; Schema: public; Owner: ipefa_iracanyes
--

CREATE TABLE IF NOT EXISTS public."Addresses" (
    "Id" uuid NOT NULL,
    "Street" text NOT NULL,
    "City" text NOT NULL,
    "State" text NOT NULL,
    "ZipCode" text NOT NULL,
    "Country" text NOT NULL,
    CONSTRAINT "PK_Addresses" PRIMARY KEY ("Id")
);


ALTER TABLE public."Addresses" OWNER TO ipefa_iracanyes;

CREATE INDEX IF NOT EXISTS  "IX_Addresses_Street_City_State" ON public."Addresses" USING btree ("Street", "City", "State");

---------------------------------------------------------------------------------------------------------------------------------
-- Personne de contact : Table, index et contraintes
---------------------------------------------------------------------------------------------------------------------------------

-- Name: Contacts; Type: TABLE; Schema: public; Owner: ipefa_iracanyes
--

CREATE TABLE IF NOT EXISTS public."Contacts" (
    "Id" uuid NOT NULL,
    "Firstname" text NOT NULL,
    "Lastname" text NOT NULL,
    -- Contrainte => Registre national unique
    "RegistryNumber" text UNIQUE NOT NULL,
    "Email" text,
    "PhoneNumber" text,
    "MobileNumber" text,
    "AddressId" uuid NOT NULL,
    CONSTRAINT "PK_Contacts" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Contacts_Addresses_AddressId" FOREIGN KEY ("AddressId") REFERENCES public."Addresses"("Id") ON DELETE CASCADE,
    -- Contrainte => registre_nat : 16 caractères
    CONSTRAINT "Check_Contacts_RegistryNumber_Length" CHECK ((char_length("RegistryNumber") <= 16)),
    -- Contrainte => registre_nat : yy.mm.dd-999.99
    CONSTRAINT "Check_Contacts_RegistryNumber_Valid" CHECK ( "RegistryNumber" ~* '^(\d{2})\.(0[1-9]|1[0-2])\.(0[1-9]|[1-2]\d|3[0-1])-(\d{3})\.(\d{2})$' ),
    -- Contrainte => Nom et prénom doivent être de format valide (au moins 2 caractères)
    CONSTRAINT "Check_Contacts_Firstname_Lastname" CHECK ( (char_length("Firstname") >= 2) AND (char_length("Lastname") >= 2 ) ),
    -- Contrainte => Email de la personne de contact doit être un format valide
    CONSTRAINT "Check_Contacts_Email_Valid" CHECK ( "Email" ~* '^[A-Za-z0-9.%!#$/?^|~]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$' )
);


ALTER TABLE public."Contacts" OWNER TO ipefa_iracanyes;

CREATE UNIQUE INDEX IF NOT EXISTS  "IX_Contacts_Email" ON public."Contacts" USING btree ("Email");

CREATE UNIQUE INDEX IF NOT EXISTS  "IX_Contacts_RegistryNumber" ON public."Contacts" USING btree ("RegistryNumber");

CREATE INDEX IF NOT EXISTS  "IX_Contacts_AddressId" ON public."Contacts" USING btree ("AddressId");


---------------------------------------------------------------------------------------------------------------------------------
-- Rôle : Table, index et contraintes
---------------------------------------------------------------------------------------------------------------------------------

-- Name: Roles; Type: TABLE; Schema: public; Owner: ipefa_iracanyes
--

CREATE TABLE IF NOT EXISTS public."Roles" (
    "Id" uuid NOT NULL,
    "Name" text NOT NULL,
    CONSTRAINT "PK_Roles" PRIMARY KEY ("Id"),
    -- Contrainte => Rol_nom: {'benevole', 'adoptant', 'candidat’, ‘Famille_accueil'}
    CONSTRAINT "Check_Role_Name_Valid" CHECK ( "Name" IN ('benevole', 'adoptant', 'candidat', 'famille_accueil', 'autres') )
);

ALTER TABLE public."Roles" OWNER TO ipefa_iracanyes;

CREATE UNIQUE INDEX IF NOT EXISTS  "IX_Roles_Name" ON public."Roles" USING btree ("Name");

INSERT INTO public."Roles" ("Id", "Name")
VALUES (gen_random_uuid(), 'benevole'),
       (gen_random_uuid(), 'candidat'),
       (gen_random_uuid(), 'adoptant'),
       (gen_random_uuid(), 'famille_accueil'),
       (gen_random_uuid(), 'autres');

---------------------------------------------------------------------------------------------------------------------------------
-- Contact rôle : Table, index et contraintes
---------------------------------------------------------------------------------------------------------------------------------

-- Name: ContactRoles; Type: TABLE; Schema: public; Owner: ipefa_iracanyes
--

CREATE TABLE IF NOT EXISTS public."ContactRoles" (
    "Id" uuid NOT NULL,
    "ContactId" uuid NOT NULL,
    "RoleId" uuid NOT NULL,
    CONSTRAINT "PK_ContactRoles" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_ContactRoles_Contacts_ContactId" FOREIGN KEY ("ContactId") REFERENCES public."Contacts"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_ContactRoles_Roles_RoleId" FOREIGN KEY ("RoleId") REFERENCES public."Roles"("Id") ON DELETE CASCADE

);

ALTER TABLE public."ContactRoles" OWNER TO ipefa_iracanyes;

CREATE INDEX IF NOT EXISTS  "IX_ContactRoles_ContactId" ON public."ContactRoles" USING btree ("ContactId");

CREATE INDEX IF NOT EXISTS  "IX_ContactRoles_RoleId" ON public."ContactRoles" USING btree ("RoleId");

---------------------------------------------------------------------------------------------------------------------------------
-- Entrée : Table, index et contraintes
---------------------------------------------------------------------------------------------------------------------------------

-- Name: Admissions; Type: TABLE; Schema: public; Owner: ipefa_iracanyes
--

CREATE TABLE IF NOT EXISTS public."Admissions" (
    "Id" uuid NOT NULL,
    "Reason" text NOT NULL,
    "DateCreated" timestamp with time zone NOT NULL,
    "ContactId" uuid NOT NULL,
    "AnimalId" character varying(11) NOT NULL,
    CONSTRAINT "PK_Admissions" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Admissions_Animals_AnimalId" FOREIGN KEY ("AnimalId") REFERENCES public."Animals"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Admissions_Contacts_ContactId" FOREIGN KEY ("ContactId") REFERENCES public."Contacts"("Id") ON DELETE CASCADE,
    -- Contrainte => raison: {'abandon', 'errant', 'deces_proprietaire', 'saisie', 'retour_adoption’, ‘retour_famille_accueil’}
    CONSTRAINT "Check_Admissions_Reason_Valid" CHECK ( "Reason" IN ('abandon', 'errant', 'deces_proprietaire', 'saisie', 'retour_adoption', 'retour_famille_accueil') )
);

ALTER TABLE public."Admissions" OWNER TO ipefa_iracanyes;

-- Index pour la relation : Admission  - ManyToOne - Animal
CREATE INDEX IF NOT EXISTS "IX_Admissions_AnimalId" ON public."Admissions" USING btree ("AnimalId");
-- Index pour la relation : Admission  - ManyToOne - Contact
CREATE INDEX IF NOT EXISTS  "IX_Admissions_ContactId" ON public."Admissions" USING btree ("ContactId");



---------------------------------------------------------------------------------------------------------------------------------
-- Sorties : Table, index et contraintes
---------------------------------------------------------------------------------------------------------------------------------

-- Name: Releases; Type: TABLE; Schema: public; Owner: ipefa_iracanyes
--

CREATE TABLE IF NOT EXISTS public."Releases" (
    "Id" uuid NOT NULL,
    "Reason" text NOT NULL,
    "DateCreated" timestamp with time zone NOT NULL,
    "AnimalId" varchar(11) NOT NULL,
    "ContactId" uuid NOT NULL,
    CONSTRAINT "PK_Releases" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Releases_Animals_AnimalId" FOREIGN KEY ("AnimalId") REFERENCES public."Animals"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Releases_Contacts_ContactId" FOREIGN KEY ("ContactId") REFERENCES public."Contacts"("Id") ON DELETE CASCADE,
    -- Contrainte => raison: {'adoption', 'retour_proprietaire', 'deces_animal’, ‘famille_accueil’}
    CONSTRAINT "Check_Releases_Reason" CHECK ( "Reason" IN ('adoption', 'retour_proprietaire', 'deces_animal', 'famille_accueil') )
);


ALTER TABLE public."Releases" OWNER TO ipefa_iracanyes;

CREATE INDEX IF NOT EXISTS  "IX_Releases_AnimalId" ON public."Releases" USING btree ("AnimalId");

CREATE INDEX IF NOT EXISTS  "IX_Releases_ContactId" ON public."Releases" USING btree ("ContactId");




---------------------------------------------------------------------------------------------------------------------------------
-- Adoption : Table, index et contraintes
---------------------------------------------------------------------------------------------------------------------------------

-- Name: Adoptions; Type: TABLE; Schema: public; Owner: ipefa_iracanyes
--

CREATE TABLE IF NOT EXISTS public."Adoptions" (
    "Id" uuid NOT NULL,
    "Status" text NOT NULL,
    "DateCreated" timestamp with time zone NOT NULL,
    "DateStart" date NOT NULL,
    "DateEnd" date,
    "ContactId" uuid NOT NULL,
    "AnimalId" varchar(11) NOT NULL,
    CONSTRAINT "PK_Adoptions" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Adoptions_Contacts_ContactId" FOREIGN KEY ("ContactId") REFERENCES public."Contacts"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Adoptions_Animals_AnimalId" FOREIGN KEY ("AnimalId") REFERENCES public."Animals"("Id") ON DELETE CASCADE,
    -- Contrainte => statut: {'demande', 'acceptee', 'rejet_environnement', 'rejet_comportement'}
    CONSTRAINT "Check_Adoptions_Status_Valid" CHECK ( "Status" IN ('demande', 'acceptee', 'rejet_environnement', 'rejet_comportement'))
);


ALTER TABLE public."Adoptions" OWNER TO ipefa_iracanyes;

CREATE INDEX IF NOT EXISTS  "IX_Adoptions_AnimalId" ON public."Adoptions" USING btree ("AnimalId");

CREATE UNIQUE INDEX IF NOT EXISTS  "IX_Adoptions_ContactId" ON public."Adoptions" USING btree ("ContactId");



---------------------------------------------------------------------------------------------------------------------------------
-- Compatibilité : Table, index et contraintes
---------------------------------------------------------------------------------------------------------------------------------

-- Name: Compatibilities; Type: TABLE; Schema: public; Owner: ipefa_iracanyes
--

CREATE TABLE IF NOT EXISTS public."Compatibilities" (
    "Id" uuid NOT NULL,
    "Type" text NOT NULL,
    CONSTRAINT "PK_Compatibilities" PRIMARY KEY ("Id")
);


ALTER TABLE public."Compatibilities" OWNER TO ipefa_iracanyes;

CREATE UNIQUE INDEX IF NOT EXISTS  "IX_Compatibilities_CompatibilityType" ON public."Compatibilities" USING btree ("Type");

INSERT INTO public."Compatibilities" ("Id", "Type")
VALUES (gen_random_uuid(), 'chat'),
       (gen_random_uuid(), 'chien'),
       (gen_random_uuid(), 'jeune enfant'),
       (gen_random_uuid(), 'enfant'),
       (gen_random_uuid(), 'jardin'),
       (gen_random_uuid(), 'poney');

---------------------------------------------------------------------------------------------------------------------------------
-- AnimalCompatibilité : Table, index et contraintes
---------------------------------------------------------------------------------------------------------------------------------
-- Name: AnimalCompatibilities; Type: TABLE; Schema: public; Owner: ipefa_iracanyes
--

CREATE TABLE IF NOT EXISTS public."AnimalCompatibilities" (
    "Id" uuid NOT NULL,
    "Value" boolean,
    "Description" text,
    "AnimalId" character varying(11) NOT NULL,
    "CompatibilityId" uuid NOT NULL,
    CONSTRAINT "PK_AnimalCompatibilities" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_AnimalCompatibilities_Animals_AnimalId" FOREIGN KEY ("AnimalId") REFERENCES public."Animals"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_AnimalCompatibilities_Compatibilities_CompatibilityId" FOREIGN KEY ("CompatibilityId") REFERENCES public."Compatibilities"("Id") ON DELETE CASCADE

);


ALTER TABLE public."AnimalCompatibilities" OWNER TO ipefa_iracanyes;


CREATE INDEX "IX_AnimalCompatibilities_AnimalId" ON public."AnimalCompatibilities" USING btree ("AnimalId");

CREATE INDEX IF NOT EXISTS  "IX_AnimalCompatibilities_CompatibilityId" ON public."AnimalCompatibilities" USING btree ("CompatibilityId");


---------------------------------------------------------------------------------------------------------------------------------
-- Famille d'accueil : Table, index et contraintes
---------------------------------------------------------------------------------------------------------------------------------

-- Name: FosterFamilies; Type: TABLE; Schema: public; Owner: ipefa_iracanyes
--

CREATE TABLE IF NOT EXISTS public."FosterFamilies" (
    "Id" uuid NOT NULL,
    "DateCreated" timestamp with time zone NOT NULL,
    "DateStart" date NOT NULL,
    "DateEnd" date,
    "AnimalId" varchar(11) NOT NULL,
    "ContactId" uuid NOT NULL,
    CONSTRAINT "PK_FosterFamilies" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_FosterFamilies_Contacts_ContactId" FOREIGN KEY ("ContactId") REFERENCES public."Contacts"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_FosterFamilies_Animals_AnimalId" FOREIGN KEY ("AnimalId") REFERENCES public."Animals"("Id") ON DELETE CASCADE,
    CONSTRAINT "Check_DateEnd_Valid" CHECK ( "DateEnd" IS NULL OR "DateEnd" > "DateStart" )
);


ALTER TABLE public."FosterFamilies" OWNER TO ipefa_iracanyes;

CREATE INDEX IF NOT EXISTS  "IX_FosterFamilies_ContactId" ON public."FosterFamilies" USING btree ("ContactId");

CREATE INDEX IF NOT EXISTS  "IX_FosterFamilies_AnimalId" ON public."FosterFamilies" USING btree ("AnimalId");


---------------------------------------------------------------------------------------------------------------------------------
-- Vaccin : Table, index et contraintes
---------------------------------------------------------------------------------------------------------------------------------

-- Name: Vaccines; Type: TABLE; Schema: public; Owner: ipefa_iracanyes
--

CREATE TABLE IF NOT EXISTS public."Vaccines" (
    "Id" uuid NOT NULL,
    "Name" text NOT NULL,
    CONSTRAINT "PK_Vaccines" PRIMARY KEY ("Id")
);


ALTER TABLE public."Vaccines" OWNER TO ipefa_iracanyes;

CREATE UNIQUE INDEX IF NOT EXISTS  "IX_Vaccines_Name" ON public."Vaccines" USING btree ("Name");

---------------------------------------------------------------------------------------------------------------------------------
-- Vaccination : Table, index et contraintes
---------------------------------------------------------------------------------------------------------------------------------

-- Name: Vaccinations; Type: TABLE; Schema: public; Owner: ipefa_iracanyes
--

CREATE TABLE IF NOT EXISTS public."Vaccinations" (
    "Id" uuid NOT NULL,
    "DateCreated" timestamptz NOT NULL,
    "DateVaccination" date NOT NULL,
    "Done" boolean NOT NULL,
    "AnimalId" character varying(11) NOT NULL,
    "VaccineId" uuid NOT NULL,
    CONSTRAINT "PK_Vaccinations" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Vaccinations_Animals_AnimalId" FOREIGN KEY ("AnimalId") REFERENCES public."Animals"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Vaccinations_Vaccines_VaccineId" FOREIGN KEY ("VaccineId") REFERENCES public."Vaccines"("Id") ON DELETE CASCADE
);


ALTER TABLE public."Vaccinations" OWNER TO ipefa_iracanyes;

CREATE INDEX IF NOT EXISTS  "IX_Vaccinations_AnimalId" ON public."Vaccinations" USING btree ("AnimalId");

CREATE INDEX IF NOT EXISTS  "IX_Vaccinations_VaccineId" ON public."Vaccinations" USING btree ("VaccineId");
































