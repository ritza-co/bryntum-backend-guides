<% const productDescription = isCalendar ?
`a performant, highly customizable JavaScript UI component with multiple views` : isGantt ?
`a performant, highly customizable JavaScript UI component for project management and scheduling` : isScheduler ?
`a performant, highly customizable JavaScript UI component for resource scheduling` : isSchedulerPro ?
`an advanced, performant JavaScript scheduling component with constraint-based scheduling` : isGrid ?
`a performant, feature-rich data grid that you can integrate with your frontend and backend stack` : isTaskBoard ?
`a performant, highly customizable Kanban-style JavaScript UI component` : '';

const exampleDataDescription = isCalendar ? `example events and resources` : isGantt ?
`example project tasks and dependencies` : isScheduler ? `example events, resources, and assignments` : isSchedulerPro ?
`example events, resources, assignments, and dependencies` : isGrid ? `example ice hockey player information` :
isTaskBoard ? `example tasks, resources, and assignments` : ''; %>

# How to use pkgBryntumTitle with Laravel and SQLite

[pkgBryntumTitle](https://bryntum.com/products/pkgname/) is <%= productDescription %>. This guide covers how to use
pkgBryntumTitle with [Laravel](https://laravel.com/), the open-source PHP-based web application framework.

<% if (isGrid) { %> We'll build an application to display example ice hockey player information in a table. <% } else {
%> We'll build an application that loads and syncs <%= exampleDataDescription %> using a local SQLite database and a
vanilla JavaScript frontend. <% } %>

We cover the following:

- Setting up a Laravel project that uses a SQLite database.
- Defining the Eloquent ORM models and database migrations needed for the example data.
- Seeding the SQLite database with example JSON data.
- Creating API endpoints to load data and sync data changes to the database.
- Building a vanilla JavaScript pkgBryntumTitle frontend using Vite.
- Configuring the pkgBryntumTitle frontend to load data from the database and synchronize changes to the database using
  the created API endpoints.

Here's what you'll build:

![Complete pkgBryntumTitle with example data](data/pkgName/images/integration/backends/laravel/complete-app.png)

You can find the code for the completed guide in our GitHub repositories:

- [Laravel pkgBryntumTitle backend](https://github.com/bryntum/bryntum-backend-guides/tree/main/backend/laravel/sqlite-pkgname)
- [pkgName Vite frontend](https://github.com/bryntum/bryntum-backend-guides/tree/main/frontend/vanilla-js/pkgname)

## Prerequisites

To follow along, you need [PHP](https://php.net/), [Composer](https://getcomposer.org/),
[Laravel installer](https://github.com/laravel/installer), and [Node.js](https://nodejs.org/en/download) installed on
your system. The Laravel docs guide to
[installing PHP and the Laravel installer](https://laravel.com/docs/#installing-php) shows you how to install PHP,
Composer, and the Laravel installer with a single line of commands.

## Getting started

We'll create a Laravel app using the Laravel installer.

### Creating a Laravel app using the Laravel installer

Run the Laravel installer to create a new Laravel app:

```bash
laravel new bryntum-pkgname-backend
```

Answer the prompts in the terminal as follows:

- **Which starter kit would you like to install?**

  `None`

- **Which database will your application use?**

  `SQLite`

- **Would you like to run npm install and npm run build?**

  `Yes`

Change the directory to your app's folder:

```bash
cd bryntum-pkgname-backend
```

### Configuring the Composer scripts

Update the `"dev"` script in `composer.json` so the backend runs on port `1337`:

```json
"dev": [
    "Composer\\Config::disableProcessTimeout",
    "php artisan serve --port=1337"
],
```

Because we create the frontend in a separate Vite project, we replace the default `"dev"` script (which runs multiple
processes including Vite) with one that only starts the Laravel development server on port `1337`.

### Enabling API routes

Create an empty `routes/api.php` file for the API routes:

```bash
touch routes/api.php
```

Update the `withRouting` call in `bootstrap/app.php` so Laravel loads the `routes/api.php` file:

```php
<?php

use Illuminate\Foundation\Application;
use Illuminate\Foundation\Configuration\Exceptions;
use Illuminate\Foundation\Configuration\Middleware;

return Application::configure(basePath: dirname(__DIR__))
    ->withRouting(
        web: __DIR__.'/../routes/web.php',
        api: __DIR__.'/../routes/api.php',
        commands: __DIR__.'/../routes/console.php',
        health: '/up',
    )
    ->withMiddleware(function (Middleware $middleware): void {
        //
    })
    ->withExceptions(function (Exceptions $exceptions): void {
        //
    })->create();
```

This registers the `routes/api.php` file for your API. All routes defined in this file are automatically prefixed with
`/api`. We add the route definitions after creating the controller.

### Configuring CORS

Because the frontend runs on a different port to the backend, we need to configure
[Cross-Origin Resource Sharing (CORS)](https://developer.mozilla.org/en-US/docs/Web/HTTP/CORS) so the browser allows API
requests from the frontend.

Publish the CORS configuration file:

```bash
php artisan config:publish cors
```

Open the created `config/cors.php` file and make sure the `paths`, `allowed_methods`, `allowed_origins`, and
`allowed_headers` settings allow requests from the frontend:

```php
<?php

return [

    'paths' => ['api/*', 'sanctum/csrf-cookie'],

    'allowed_methods' => ['*'],

    'allowed_origins' => ['*'],

    'allowed_origins_patterns' => [],

    'allowed_headers' => ['*'],

    'exposed_headers' => [],

    'max_age' => 0,

    'supports_credentials' => false,

];
```

This allows all origins, methods, and headers for the `api/*` routes. For production, you should restrict
`allowed_origins` to your frontend's domain.

## Creating the data models

<% if (isCalendar) { %>We'll define Eloquent models for the example events and resources data. In Bryntum Calendar, data
stores are kept and linked together in the [project](#Calendar/guides/data/displayingdata.md#the-calendar-project). This
tutorial covers creating the model and migration files for the EventStore and the ResourceStore. <% } %>

<% if (isGantt) { %>We'll define Eloquent models for the example project data. In Bryntum Gantt, data is managed through
a [project](#Gantt/model/ProjectModel) that contains stores for tasks, dependencies, resources, assignments, time
ranges, and calendars. This tutorial covers creating the model and migration files for the TaskStore and the
DependencyStore. <% } %>

<% if (isScheduler) { %>We'll define Eloquent models for the example events, resources, and assignments data. In Bryntum
Scheduler, data stores are kept and linked together in the [Crud Manager](#Scheduler/guides/data/crud_manager.md). This
tutorial covers creating the model and migration files for the EventStore, ResourceStore, and AssignmentStore. <% } %>

<% if (isSchedulerPro) { %>We'll define Eloquent models for the example events, resources, assignments, and dependencies
data. In Bryntum Scheduler Pro, data stores are kept and linked together in the
[project](#SchedulerPro/model/ProjectModel). This tutorial covers creating the model and migration files for the
EventStore, ResourceStore, AssignmentStore, and DependencyStore. <% } %>

<% if (isGrid) { %>Now let's create an Eloquent ORM model for the ice hockey players' data. In Bryntum Grid, data is
managed through a [Store](#Core/data/Store). This tutorial covers creating the model and migration for the data store.
<% } %>

<% if (isTaskBoard) { %>We'll define Eloquent models for the example tasks, resources, and assignments data. In Bryntum
Task Board, data is managed through a [project](#TaskBoard/model/ProjectModel) that contains stores for tasks,
resources, and assignments. This tutorial covers creating the model and migration files for those three stores. <% } %>

<% if (isGrid) { %>

### Creating the Player model

Run the following Artisan command to create a `Player` model:

```bash
php artisan make:model Player
```

Open the `app/Models/Player.php` file and replace its contents with the following lines of code:

```php
<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

/**
 * @property int $id
 * @property string $name
 * @property string $city
 * @property string $team
 * @property float $score
 * @property float $percentageWins
 */

class Player extends Model
{
    public $timestamps = false;

    protected $fillable = [
        'name',
        'city',
        'team',
        'score',
        'percentageWins',
    ];

    protected $casts = [
        'score'          => 'float',
        'percentageWins' => 'float',
    ];
}
```

### Creating the Player migration

Run the following command to generate a [database migration](https://laravel.com/docs/12.x/migrations) for the players
table:

```bash
php artisan make:migration create_players_table
```

Open the created migration file in the `database/migrations/` directory and replace its contents with the following
code:

```php
<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    /**
     * Run the migrations.
     */
    public function up(): void
    {
        Schema::create('players', function (Blueprint $table) {
            $table->id();
            $table->string('name')->nullable();
            $table->string('city')->nullable();
            $table->string('team')->nullable();
            $table->float('score')->default(0);
            $table->float('percentageWins')->default(0);
        });
    }

    /**
     * Reverse the migrations.
     */
    public function down(): void
    {
        Schema::dropIfExists('players');
    }
};
```

<% } %>

<% if (isCalendar) { %>

### Creating the Resource model

Run the following Artisan command to create a `Resource` model:

```bash
php artisan make:model Resource
```

Open the `app/Models/Resource.php` file and replace its contents with the following lines of code:

```php
<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Model;

class Resource extends Model
{
    use HasFactory;

    protected $fillable = [
        'id',
        'name',
        'eventColor',
        'readOnly',
    ];

    protected $casts = [
        'readOnly' => 'boolean',
    ];

    public $timestamps = false;
    public $incrementing = false;
    protected $keyType = 'string';

    public function events()
    {
        return $this->hasMany(Event::class, 'resourceId');
    }
}
```

### Creating the Event model

Run the following Artisan command to create an `Event` model:

```bash
php artisan make:model Event
```

Open the `app/Models/Event.php` file and replace its contents with the following lines of code:

```php
<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Model;

class Event extends Model
{
    use HasFactory;

    protected $fillable = [
        'name',
        'startDate',
        'endDate',
        'allDay',
        'resourceId',
        'eventColor',
        'readOnly',
        'timeZone',
        'draggable',
        'resizable',
        'duration',
        'durationUnit',
        'exceptionDates',
        'recurrenceRule',
        'cls',
        'eventStyle',
        'iconCls',
        'style',
    ];

    protected $casts = [
        'allDay' => 'boolean',
        'readOnly' => 'boolean',
        'draggable' => 'boolean',
        'resizable' => 'string',
        'duration' => 'integer',
        'exceptionDates' => 'json',
    ];

    public $timestamps = false;

    public function resource()
    {
        return $this->belongsTo(Resource::class, 'resourceId');
    }

    protected function serializeDate(\DateTimeInterface $date): string
    {
        return $date->format('Y-m-d\\TH:i:s.000\\Z');
    }

    public function toArray()
    {
        $array = parent::toArray();

        if (isset($array['startDate']) && $array['startDate']) {
            $array['startDate'] = \Carbon\Carbon::parse($array['startDate'])->utc()->format('Y-m-d\\TH:i:s.000\\Z');
        }

        if (isset($array['endDate']) && $array['endDate']) {
            $array['endDate'] = \Carbon\Carbon::parse($array['endDate'])->utc()->format('Y-m-d\\TH:i:s.000\\Z');
        }

        return $array;
    }
}
```

### Creating the migrations

Run the following commands to generate the database migrations:

```bash
php artisan make:migration create_resources_table
php artisan make:migration create_events_table
```

Open the `create_resources_table` migration file in `database/migrations/` and replace its contents with the following
code:

```php
<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    /**
     * Run the migrations.
     */
    public function up(): void
    {
        Schema::create('resources', function (Blueprint $table) {
            $table->string('id')->primary();
            $table->string('name');
            $table->string('eventColor')->nullable();
            $table->boolean('readOnly')->default(false);
        });
    }

    /**
     * Reverse the migrations.
     */
    public function down(): void
    {
        Schema::dropIfExists('resources');
    }
};
```

This migration defines the `resources` table with a string primary key for the resource ID.

Open the `create_events_table` migration file and replace its contents with the following code:

```php
<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    /**
     * Run the migrations.
     */
    public function up(): void
    {
        Schema::create('events', function (Blueprint $table) {
            $table->id();
            $table->string('name');
            $table->datetime('startDate')->nullable();
            $table->datetime('endDate')->nullable();
            $table->boolean('allDay')->default(false);
            $table->string('resourceId')->nullable();
            $table->string('eventColor')->nullable();
            $table->boolean('readOnly')->default(false);
            $table->string('timeZone')->nullable();
            $table->boolean('draggable')->default(true);
            $table->string('resizable')->default('true');
            $table->integer('duration')->nullable();
            $table->string('durationUnit')->default('day');
            $table->json('exceptionDates')->nullable();
            $table->string('recurrenceRule')->nullable();
            $table->string('cls')->nullable();
            $table->string('eventStyle')->nullable();
            $table->string('iconCls')->nullable();
            $table->string('style')->nullable();

            $table->foreign('resourceId')->references('id')->on('resources')->onDelete('set null');
        });
    }

    /**
     * Reverse the migrations.
     */
    public function down(): void
    {
        Schema::dropIfExists('events');
    }
};
```

This migration defines the `events` table with a foreign key to the `resources` table.

<% } %>

<% if (isGantt) { %>

### Creating the Task model

Run the following Artisan commands to create the models:

```bash
php artisan make:model Task
php artisan make:model Dependency
```

Open the `app/Models/Task.php` file and replace its contents with the following lines of code:

```php
<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Model;

class Task extends Model
{
    use HasFactory;

    protected $fillable = [
        'name',
        'startDate',
        'endDate',
        'duration',
        'percentDone',
        'parentId',
        'expanded',
        'rollup',
        'manuallyScheduled',
        'parentIndex',
        'effort',
    ];

    protected $casts = [
        'startDate' => 'datetime',
        'endDate' => 'datetime',
        'duration' => 'float',
        'percentDone' => 'float',
        'parentId' => 'integer',
        'expanded' => 'boolean',
        'rollup' => 'boolean',
        'manuallyScheduled' => 'boolean',
        'parentIndex' => 'integer',
        'effort' => 'integer',
    ];

    public $timestamps = false;

    public function children()
    {
        return $this->hasMany(Task::class, 'parentId');
    }

    public function parent()
    {
        return $this->belongsTo(Task::class, 'parentId');
    }

    protected function serializeDate(\DateTimeInterface $date): string
    {
        return $date->format('Y-m-d\\TH:i:s.000\\Z');
    }

    public function toArray()
    {
        $array = parent::toArray();

        if (isset($array['startDate']) && $array['startDate']) {
            $array['startDate'] = \Carbon\Carbon::parse($array['startDate'])->utc()->format('Y-m-d\\TH:i:s.000\\Z');
        }

        if (isset($array['endDate']) && $array['endDate']) {
            $array['endDate'] = \Carbon\Carbon::parse($array['endDate'])->utc()->format('Y-m-d\\TH:i:s.000\\Z');
        }

        return $array;
    }
}
```

### Creating the Dependency model

Open the `app/Models/Dependency.php` file and replace its contents with the following lines of code:

```php
<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Model;

class Dependency extends Model
{
    use HasFactory;

    protected $fillable = [
        'fromEvent',
        'toEvent',
        'type',
        'cls',
        'lag',
        'lagUnit',
        'active',
        'fromSide',
        'toSide',
    ];

    protected $casts = [
        'fromEvent' => 'integer',
        'toEvent' => 'integer',
        'type' => 'integer',
        'lag' => 'float',
        'active' => 'boolean',
    ];

    public $timestamps = false;
}
```

### Creating the migrations

Run the following commands to generate the database migrations:

```bash
php artisan make:migration create_tasks_table
php artisan make:migration create_dependencies_table
```

Open the `create_tasks_table` migration file in `database/migrations/` and replace its contents with the following code:

```php
<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    /**
     * Run the migrations.
     */
    public function up(): void
    {
        Schema::create('tasks', function (Blueprint $table) {
            $table->id();
            $table->string('name');
            $table->dateTime('startDate')->nullable();
            $table->dateTime('endDate')->nullable();
            $table->float('duration')->nullable();
            $table->float('percentDone')->default(0);
            $table->unsignedBigInteger('parentId')->nullable();
            $table->boolean('expanded')->default(true);
            $table->boolean('rollup')->default(false);
            $table->boolean('manuallyScheduled')->default(false);
            $table->integer('parentIndex')->nullable();
            $table->integer('effort')->nullable();

            $table->foreign('parentId')->references('id')->on('tasks')->onDelete('cascade');
        });
    }

    /**
     * Reverse the migrations.
     */
    public function down(): void
    {
        Schema::dropIfExists('tasks');
    }
};
```

This migration defines the `tasks` table with a self-referencing `parentId` foreign key that enables the hierarchical
task structure. Cascade delete ensures child tasks are removed when a parent is deleted.

Open the `create_dependencies_table` migration file and replace its contents with the following code:

```php
<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    /**
     * Run the migrations.
     */
    public function up(): void
    {
        Schema::create('dependencies', function (Blueprint $table) {
            $table->id();
            $table->unsignedBigInteger('fromEvent')->nullable();
            $table->unsignedBigInteger('toEvent')->nullable();
            $table->integer('type')->default(2);
            $table->string('cls')->nullable();
            $table->float('lag')->default(0);
            $table->string('lagUnit')->default('day');
            $table->boolean('active')->default(true);
            $table->string('fromSide')->nullable();
            $table->string('toSide')->nullable();

            $table->index('fromEvent');
            $table->index('toEvent');
            $table->foreign('fromEvent')->references('id')->on('tasks')->onDelete('cascade');
            $table->foreign('toEvent')->references('id')->on('tasks')->onDelete('cascade');
        });
    }

    /**
     * Reverse the migrations.
     */
    public function down(): void
    {
        Schema::dropIfExists('dependencies');
    }
};
```

<% } %>

<% if (isScheduler) { %>

### Creating the Resource model

Run the following Artisan commands to create the models:

```bash
php artisan make:model Resource
php artisan make:model Event
php artisan make:model Assignment
```

Open the `app/Models/Resource.php` file and replace its contents with the following lines of code:

```php
<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Model;

class Resource extends Model
{
    use HasFactory;

    protected $fillable = [
        'name',
        'eventColor',
        'readOnly',
    ];

    protected $casts = [
        'readOnly' => 'boolean',
    ];

    public $timestamps = false;

    public function assignments()
    {
        return $this->hasMany(Assignment::class, 'resourceId');
    }

    public function events()
    {
        return $this->belongsToMany(Event::class, 'assignments', 'resourceId', 'eventId');
    }
}
```

### Creating the Event model

Open the `app/Models/Event.php` file and replace its contents with the following lines of code:

```php
<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Casts\Attribute;
use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Model;

class Event extends Model
{
    use HasFactory;

    protected $fillable = [
        'name',
        'startDate',
        'endDate',
        'allDay',
        'eventColor',
        'readOnly',
        'timeZone',
        'draggable',
        'resizable',
        'children',
        'duration',
        'durationUnit',
        'exceptionDates',
        'recurrenceRule',
        'cls',
        'eventStyle',
        'iconCls',
        'style',
    ];

    protected $casts = [
        'startDate' => 'datetime:Y-m-d\\TH:i:s',
        'endDate' => 'datetime:Y-m-d\\TH:i:s',
        'allDay' => 'boolean',
        'readOnly' => 'boolean',
        'draggable' => 'boolean',
        'duration' => 'float',
        'exceptionDates' => 'json',
    ];

    public $timestamps = false;

    public function assignments()
    {
        return $this->hasMany(Assignment::class, 'eventId');
    }

    public function resources()
    {
        return $this->belongsToMany(Resource::class, 'assignments', 'eventId', 'resourceId');
    }

    protected function resizable(): Attribute
    {
        return Attribute::make(
            get: static function ($value) {
                if ($value === null) {
                    return null;
                }

                if (is_bool($value)) {
                    return $value;
                }

                if (is_numeric($value)) {
                    return (int) $value === 1;
                }

                $normalized = strtolower((string) $value);

                return match ($normalized) {
                    'true' => true,
                    'false' => false,
                    default => $value,
                };
            },
            set: static function ($value) {
                if (is_bool($value)) {
                    return $value ? 'true' : 'false';
                }

                return $value;
            }
        );
    }
}
```

### Creating the Assignment model

Open the `app/Models/Assignment.php` file and replace its contents with the following lines of code:

```php
<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Model;

class Assignment extends Model
{
    use HasFactory;

    protected $fillable = [
        'eventId',
        'resourceId',
    ];

    protected $casts = [
        'eventId' => 'integer',
        'resourceId' => 'integer',
    ];

    public $timestamps = false;

    public function event()
    {
        return $this->belongsTo(Event::class, 'eventId');
    }

    public function resource()
    {
        return $this->belongsTo(Resource::class, 'resourceId');
    }
}
```

### Creating the migrations

Run the following commands to generate the database migrations:

```bash
php artisan make:migration create_resources_table
php artisan make:migration create_events_table
php artisan make:migration create_assignments_table
php artisan make:migration change_event_duration_to_double
```

Open the `create_resources_table` migration file in `database/migrations/` and replace its contents with the following
code:

```php
<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    /**
     * Run the migrations.
     */
    public function up(): void
    {
        Schema::create('resources', function (Blueprint $table) {
            $table->string('id')->primary();
            $table->string('name');
            $table->string('eventColor')->nullable();
            $table->boolean('readOnly')->default(false);
        });
    }

    /**
     * Reverse the migrations.
     */
    public function down(): void
    {
        Schema::dropIfExists('resources');
    }
};
```

This migration defines the `resources` table with a string primary key for the resource ID.

Open the `create_events_table` migration file and replace its contents with the following code:

```php
<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    /**
     * Run the migrations.
     */
    public function up(): void
    {
        Schema::create('events', function (Blueprint $table) {
            $table->id();
            $table->string('name');
            $table->datetime('startDate')->nullable();
            $table->datetime('endDate')->nullable();
            $table->boolean('allDay')->default(false);
            $table->string('eventColor')->nullable();
            $table->boolean('readOnly')->default(false);
            $table->string('timeZone')->nullable();
            $table->boolean('draggable')->default(true);
            $table->string('resizable')->default('true');
            $table->string('children')->nullable();
            $table->double('duration')->nullable();
            $table->string('durationUnit')->default('day');
            $table->json('exceptionDates')->nullable();
            $table->string('recurrenceRule')->nullable();
            $table->string('cls')->nullable();
            $table->string('eventStyle')->nullable();
            $table->string('iconCls')->nullable();
            $table->string('style')->nullable();
        });
    }

    /**
     * Reverse the migrations.
     */
    public function down(): void
    {
        Schema::dropIfExists('events');
    }
};
```

This migration defines the `events` table with columns for scheduling properties, styling, and recurrence support.

Open the `create_assignments_table` migration file and replace its contents with the following code:

```php
<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    /**
     * Run the migrations.
     */
    public function up(): void
    {
        Schema::create('assignments', function (Blueprint $table) {
            $table->id();
            $table->unsignedBigInteger('eventId');
            $table->unsignedBigInteger('resourceId');

            $table->foreign('eventId')->references('id')->on('events')->onDelete('cascade');
            $table->foreign('resourceId')->references('id')->on('resources')->onDelete('cascade');

            $table->index(['eventId']);
            $table->index(['resourceId']);
        });
    }

    /**
     * Reverse the migrations.
     */
    public function down(): void
    {
        Schema::dropIfExists('assignments');
    }
};
```

This migration defines the `assignments` table that links events to resources with foreign keys and cascade delete.

Open the `change_event_duration_to_double` migration file and replace its contents with the following code:

```php
<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    /**
     * Run the migrations.
     */
    public function up(): void
    {
        Schema::table('events', function (Blueprint $table) {
            $table->double('duration')->nullable()->change();
        });
    }

    /**
     * Reverse the migrations.
     */
    public function down(): void
    {
        Schema::table('events', function (Blueprint $table) {
            $table->integer('duration')->nullable()->change();
        });
    }
};
```

<% } %>

<% if (isSchedulerPro) { %>

### Creating the Resource model

Run the following Artisan commands to create the models:

```bash
php artisan make:model Resource
php artisan make:model Event
php artisan make:model Assignment
php artisan make:model Dependency
```

Open the `app/Models/Resource.php` file and replace its contents with the following lines of code:

```php
<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Model;

class Resource extends Model
{
    use HasFactory;

    protected $fillable = [
        'name',
        'eventColor',
        'readOnly',
    ];

    protected $casts = [
        'readOnly' => 'boolean',
    ];

    public $timestamps = false;

    public function assignments()
    {
        return $this->hasMany(Assignment::class, 'resourceId');
    }

    public function events()
    {
        return $this->belongsToMany(Event::class, 'assignments', 'resourceId', 'eventId');
    }
}
```

### Creating the Event model

Open the `app/Models/Event.php` file and replace its contents with the following lines of code:

```php
<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Casts\Attribute;
use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Model;

class Event extends Model
{
    use HasFactory;

    protected $fillable = [
        'name',
        'readOnly',
        'timeZone',
        'draggable',
        'resizable',
        'children',
        'allDay',
        'duration',
        'durationUnit',
        'startDate',
        'endDate',
        'exceptionDates',
        'recurrenceRule',
        'cls',
        'eventColor',
        'eventStyle',
        'iconCls',
        'style',
    ];

    protected $casts = [
        'startDate' => 'datetime:Y-m-d\\TH:i:s',
        'endDate' => 'datetime:Y-m-d\\TH:i:s',
        'allDay' => 'boolean',
        'readOnly' => 'boolean',
        'draggable' => 'boolean',
        'duration' => 'float',
        'exceptionDates' => 'json',
    ];

    public $timestamps = false;

    public function assignments()
    {
        return $this->hasMany(Assignment::class, 'eventId');
    }

    public function resources()
    {
        return $this->belongsToMany(Resource::class, 'assignments', 'eventId', 'resourceId');
    }

    protected function resizable(): Attribute
    {
        return Attribute::make(
            get: static function ($value) {
                if ($value === null) {
                    return null;
                }

                if (is_bool($value)) {
                    return $value;
                }

                if (is_numeric($value)) {
                    return (int) $value === 1;
                }

                $normalized = strtolower((string) $value);

                return match ($normalized) {
                    'true' => true,
                    'false' => false,
                    default => $value,
                };
            },
            set: static function ($value) {
                if (is_bool($value)) {
                    return $value ? 'true' : 'false';
                }

                return $value;
            }
        );
    }
}
```

### Creating the Assignment model

Open the `app/Models/Assignment.php` file and replace its contents with the following lines of code:

```php
<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Model;

class Assignment extends Model
{
    use HasFactory;

    protected $fillable = [
        'eventId',
        'resourceId',
    ];

    public $timestamps = false;

    public function event()
    {
        return $this->belongsTo(Event::class, 'eventId');
    }

    public function resource()
    {
        return $this->belongsTo(Resource::class, 'resourceId');
    }
}
```

### Creating the Dependency model

Open the `app/Models/Dependency.php` file and replace its contents with the following lines of code:

```php
<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Model;

class Dependency extends Model
{
    use HasFactory;

    protected $fillable = [
        'from',
        'to',
        'fromSide',
        'toSide',
        'cls',
        'lag',
        'lagUnit',
    ];

    protected $casts = [
        'from' => 'integer',
        'to' => 'integer',
        'lag' => 'float',
    ];

    public $timestamps = false;

    public function fromEvent()
    {
        return $this->belongsTo(Event::class, 'from');
    }

    public function toEvent()
    {
        return $this->belongsTo(Event::class, 'to');
    }
}
```

### Creating the migrations

Run the following commands to generate the database migrations:

```bash
php artisan make:migration create_resources_table
php artisan make:migration create_events_table
php artisan make:migration create_assignments_table
php artisan make:migration create_dependencies_table
php artisan make:migration change_event_duration_to_double
```

Open the `create_resources_table` migration file in `database/migrations/` and replace its contents with the following
code:

```php
<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    /**
     * Run the migrations.
     */
    public function up(): void
    {
        Schema::create('resources', function (Blueprint $table) {
            $table->id();
            $table->string('name');
            $table->string('eventColor')->nullable();
            $table->boolean('readOnly')->default(false);
        });
    }

    /**
     * Reverse the migrations.
     */
    public function down(): void
    {
        Schema::dropIfExists('resources');
    }
};
```

This migration defines the `resources` table for people, equipment, or rooms that events can be assigned to.

Open the `create_events_table` migration file and replace its contents with the following code:

```php
<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    /**
     * Run the migrations.
     */
    public function up(): void
    {
        Schema::create('events', function (Blueprint $table) {
            $table->id();
            $table->string('name');
            $table->boolean('readOnly')->default(false);
            $table->string('timeZone')->nullable();
            $table->boolean('draggable')->default(true);
            $table->string('resizable')->default('true');
            $table->string('children')->nullable();
            $table->boolean('allDay')->default(false);
            $table->double('duration')->nullable();
            $table->string('durationUnit')->default('day');
            $table->datetime('startDate')->nullable();
            $table->datetime('endDate')->nullable();
            $table->json('exceptionDates')->nullable();
            $table->string('recurrenceRule')->nullable();
            $table->string('cls')->nullable();
            $table->string('eventColor')->nullable();
            $table->string('eventStyle')->nullable();
            $table->string('iconCls')->nullable();
            $table->string('style')->nullable();
        });
    }

    /**
     * Reverse the migrations.
     */
    public function down(): void
    {
        Schema::dropIfExists('events');
    }
};
```

This migration defines the `events` table with columns for scheduling properties, styling, and recurrence support.

Open the `create_assignments_table` migration file and replace its contents with the following code:

```php
<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    /**
     * Run the migrations.
     */
    public function up(): void
    {
        Schema::create('assignments', function (Blueprint $table) {
            $table->id();
            $table->unsignedBigInteger('eventId');
            $table->unsignedBigInteger('resourceId');

            $table->foreign('eventId')->references('id')->on('events')->onDelete('cascade');
            $table->foreign('resourceId')->references('id')->on('resources')->onDelete('cascade');

            $table->index(['eventId']);
            $table->index(['resourceId']);
        });
    }

    /**
     * Reverse the migrations.
     */
    public function down(): void
    {
        Schema::dropIfExists('assignments');
    }
};
```

This migration defines the `assignments` table that links events to resources with foreign keys and cascade delete.

Open the `create_dependencies_table` migration file and replace its contents with the following code:

```php
<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    /**
     * Run the migrations.
     */
    public function up(): void
    {
        Schema::create('dependencies', function (Blueprint $table) {
            $table->id();
            $table->unsignedBigInteger('from')->nullable();
            $table->unsignedBigInteger('to')->nullable();
            $table->string('fromSide')->default('right');
            $table->string('toSide')->default('left');
            $table->string('cls')->nullable();
            $table->float('lag')->default(0);
            $table->string('lagUnit')->default('day');

            $table->foreign('from')->references('id')->on('events')->onDelete('cascade');
            $table->foreign('to')->references('id')->on('events')->onDelete('cascade');

            $table->index(['from']);
            $table->index(['to']);
        });
    }

    /**
     * Reverse the migrations.
     */
    public function down(): void
    {
        Schema::dropIfExists('dependencies');
    }
};
```

This migration defines the `dependencies` table that stores relationships between events, with `from` and `to` foreign
keys referencing the events table.

Open the `change_event_duration_to_double` migration file and replace its contents with the following code:

```php
<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    /**
     * Run the migrations.
     */
    public function up(): void
    {
        Schema::table('events', function (Blueprint $table) {
            $table->double('duration')->nullable()->change();
        });
    }

    /**
     * Reverse the migrations.
     */
    public function down(): void
    {
        Schema::table('events', function (Blueprint $table) {
            $table->integer('duration')->nullable()->change();
        });
    }
};
```

<% } %>

<% if (isTaskBoard) { %>

### Creating the Task model

Run the following Artisan commands to create the models:

```bash
php artisan make:model Task
php artisan make:model Resource
php artisan make:model Assignment
```

Open the `app/Models/Task.php` file and replace its contents with the following lines of code:

```php
<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Model;

class Task extends Model
{
    use HasFactory;

    protected $fillable = [
        'name',
        'eventColor',
        'description',
        'weight',
        'status',
        'prio',
    ];

    protected $casts = [
        'weight' => 'integer',
    ];

    public $timestamps = false;

    public function assignments()
    {
        return $this->hasMany(Assignment::class, 'taskId');
    }
}
```

### Creating the Resource model

Open the `app/Models/Resource.php` file and replace its contents with the following lines of code:

```php
<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Model;

class Resource extends Model
{
    use HasFactory;

    protected $fillable = [
        'id',
        'name',
        'eventColor',
        'readOnly',
    ];

    protected $casts = [
        'readOnly' => 'boolean',
    ];

    public $timestamps = false;
    public $incrementing = false;
    protected $keyType = 'string';

    public function events()
    {
        return $this->hasMany(Event::class, 'resourceId');
    }
}
```

### Creating the Assignment model

Open the `app/Models/Assignment.php` file and replace its contents with the following lines of code:

```php
<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Model;

class Assignment extends Model
{
    use HasFactory;

    protected $fillable = [
        'eventId',
        'resourceId',
    ];

    protected $casts = [
        'eventId' => 'integer',
        'resourceId' => 'string',
    ];

    public $timestamps = false;

    public function task()
    {
        return $this->belongsTo(Task::class, 'eventId');
    }

    public function resource()
    {
        return $this->belongsTo(Resource::class, 'resourceId');
    }
}
```

### Creating the migrations

Run the following commands to generate the database migrations:

```bash
php artisan make:migration create_tasks_table
php artisan make:migration create_resources_table
php artisan make:migration create_assignments_table
```

Open the `create_tasks_table` migration file in `database/migrations/` and replace its contents with the following code:

```php
<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    /**
     * Run the migrations.
     */
    public function up(): void
    {
        Schema::create('tasks', function (Blueprint $table) {
            $table->id();
            $table->string('name');
            $table->string('eventColor')->nullable();
            $table->string('description')->nullable();
            $table->integer('weight')->default(1);
            $table->string('status')->default('todo');
            $table->string('prio')->default('medium');
        });
    }

    /**
     * Reverse the migrations.
     */
    public function down(): void
    {
        Schema::dropIfExists('tasks');
    }
};
```

This migration defines the `tasks` table with columns for task name, color, description, weight, status, and priority.

Open the `create_resources_table` migration file and replace its contents with the following code:

```php
<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    /**
     * Run the migrations.
     */
    public function up(): void
    {
        Schema::create('resources', function (Blueprint $table) {
            $table->string('id')->primary();
            $table->string('name');
            $table->string('eventColor')->nullable();
            $table->boolean('readOnly')->default(false);
        });
    }

    /**
     * Reverse the migrations.
     */
    public function down(): void
    {
        Schema::dropIfExists('resources');
    }
};
```

Resources represent team members that tasks can be assigned to. The `id` column uses a string primary key.

Open the `create_assignments_table` migration file and replace its contents with the following code:

```php
<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    /**
     * Run the migrations.
     */
    public function up(): void
    {
        Schema::create('assignments', function (Blueprint $table) {
            $table->id();
            $table->unsignedBigInteger('eventId');
            $table->string('resourceId');

            $table->foreign('eventId')->references('id')->on('tasks')->onDelete('cascade');
            $table->foreign('resourceId')->references('id')->on('resources')->onDelete('cascade');

            $table->index(['eventId']);
            $table->index(['resourceId']);
        });
    }

    /**
     * Reverse the migrations.
     */
    public function down(): void
    {
        Schema::dropIfExists('assignments');
    }
};
```

<% } %>

Run the `migrate:fresh` command to recreate your entire database:

```bash
php artisan migrate:fresh
```

## Populating the SQLite database with example data

Now that we have our database table structure, let's populate it with some sample data.

First copy the `example-data` folder from the completed
[Laravel pkgBryntumTitle backend](https://github.com/bryntum/bryntum-backend-guides/tree/main/backend/laravel/sqlite-pkgname)
and add it to the root of your Laravel backend.

<% if (isGrid) { %>

Run the following Artisan command to create a player seeder:

```bash
php artisan make:seeder PlayerSeeder
```

Open the `database/seeders/PlayerSeeder.php` file and replace its contents with the following code:

```php
<?php

namespace Database\Seeders;

use Illuminate\Database\Console\Seeds\WithoutModelEvents;
use Illuminate\Database\Seeder;
use App\Models\Player;

class PlayerSeeder extends Seeder
{
    /**
     * Run the database seeds.
     */
    public function run(): void
    {
        $jsonPath = base_path('example-data/players.json');

        if (!file_exists($jsonPath)) {
            throw new \Exception("Players JSON file not found at: {$jsonPath}");
        }

        $playersData = json_decode(file_get_contents($jsonPath), true);

        if (json_last_error() !== JSON_ERROR_NONE) {
            throw new \Exception("Invalid JSON in players file: " . json_last_error_msg());
        }

        foreach ($playersData as $player) {
            Player::create($player);
        }
    }
}
```

This seeder reads the player data from `example-data/players.json` and inserts each record into the database.

Update `database/seeders/DatabaseSeeder.php`:

```php
<?php

namespace Database\Seeders;

use App\Models\User;
// use Illuminate\Database\Console\Seeds\WithoutModelEvents;
use Illuminate\Database\Seeder;

class DatabaseSeeder extends Seeder
{
    /**
     * Seed the application's database.
     */
    public function run(): void
    {
        $this->call([
            PlayerSeeder::class,
        ]);
    }
}
```

<% } %>

<% if (isCalendar) { %>

Run the following Artisan commands to create the seeders:

```bash
php artisan make:seeder ResourceSeeder
php artisan make:seeder EventSeeder
```

Open the `database/seeders/ResourceSeeder.php` file and replace its contents with the following code:

```php
<?php

namespace Database\Seeders;

use Illuminate\Database\Console\Seeds\WithoutModelEvents;
use Illuminate\Database\Seeder;
use App\Models\Resource;

class ResourceSeeder extends Seeder
{
    /**
     * Run the database seeds.
     */
    public function run(): void
    {
        $jsonPath = base_path('example-data/resources.json');

        if (!file_exists($jsonPath)) {
            throw new \Exception("Resources JSON file not found at: {$jsonPath}");
        }

        $resourcesData = json_decode(file_get_contents($jsonPath), true);

        if (json_last_error() !== JSON_ERROR_NONE) {
            throw new \Exception("Invalid JSON in resources file: " . json_last_error_msg());
        }

        foreach ($resourcesData as $resource) {
            Resource::create($resource);
        }
    }
}
```

This seeder reads the resource data from `example-data/resources.json` and inserts each record into the database.

Open the `database/seeders/EventSeeder.php` file and replace its contents with the following code:

```php
<?php

namespace Database\Seeders;

use Illuminate\Database\Console\Seeds\WithoutModelEvents;
use Illuminate\Database\Seeder;
use App\Models\Event;

class EventSeeder extends Seeder
{
    /**
     * Run the database seeds.
     */
    public function run(): void
    {
        $jsonPath = base_path('example-data/events.json');

        if (!file_exists($jsonPath)) {
            throw new \Exception("Events JSON file not found at: {$jsonPath}");
        }

        $eventsData = json_decode(file_get_contents($jsonPath), true);

        if (json_last_error() !== JSON_ERROR_NONE) {
            throw new \Exception("Invalid JSON in events file: " . json_last_error_msg());
        }

        foreach ($eventsData as $event) {
            Event::create($event);
        }
    }
}
```

This seeder reads the event data from `example-data/events.json` and inserts each record into the database.

Update `database/seeders/DatabaseSeeder.php`:

```php
<?php

namespace Database\Seeders;

use App\Models\User;
// use Illuminate\Database\Console\Seeds\WithoutModelEvents;
use Illuminate\Database\Seeder;

class DatabaseSeeder extends Seeder
{
    /**
     * Seed the application's database.
     */
    public function run(): void
    {
        $this->call([
            ResourceSeeder::class,
            EventSeeder::class,
        ]);
    }
}
```

<% } %>

<% if (isGantt) { %>

Run the following Artisan commands to create the seeders:

```bash
php artisan make:seeder TaskSeeder
php artisan make:seeder DependencySeeder
```

Open the `database/seeders/TaskSeeder.php` file and replace its contents with the following code:

```php
<?php

namespace Database\Seeders;

use Illuminate\Database\Console\Seeds\WithoutModelEvents;
use Illuminate\Database\Seeder;
use App\Models\Task;

class TaskSeeder extends Seeder
{
    /**
     * Run the database seeds.
     */
    public function run(): void
    {
        $jsonPath = base_path('example-data/tasks.json');

        if (!file_exists($jsonPath)) {
            throw new \Exception("Tasks JSON file not found at: {$jsonPath}");
        }

        $tasksData = json_decode(file_get_contents($jsonPath), true);

        if (json_last_error() !== JSON_ERROR_NONE) {
            throw new \Exception("Invalid JSON in tasks file: " . json_last_error_msg());
        }

        foreach ($tasksData as $task) {
            Task::create($task);
        }
    }
}
```

This seeder reads the task data from `example-data/tasks.json` and inserts each record into the database.

Open the `database/seeders/DependencySeeder.php` file and replace its contents with the following code:

```php
<?php

namespace Database\Seeders;

use Illuminate\Database\Console\Seeds\WithoutModelEvents;
use Illuminate\Database\Seeder;
use App\Models\Dependency;

class DependencySeeder extends Seeder
{
    /**
     * Run the database seeds.
     */
    public function run(): void
    {
        $jsonPath = base_path('example-data/dependencies.json');

        if (!file_exists($jsonPath)) {
            throw new \Exception("Dependencies JSON file not found at: {$jsonPath}");
        }

        $dependenciesData = json_decode(file_get_contents($jsonPath), true);

        if (json_last_error() !== JSON_ERROR_NONE) {
            throw new \Exception("Invalid JSON in dependencies file: " . json_last_error_msg());
        }

        foreach ($dependenciesData as $dependency) {
            Dependency::create($dependency);
        }
    }
}
```

This seeder reads the dependency data from `example-data/dependencies.json` and inserts each record into the database.

Update `database/seeders/DatabaseSeeder.php`:

```php
<?php

namespace Database\Seeders;

use App\Models\User;
// use Illuminate\Database\Console\Seeds\WithoutModelEvents;
use Illuminate\Database\Seeder;

class DatabaseSeeder extends Seeder
{
    /**
     * Seed the application's database.
     */
    public function run(): void
    {
        $this->call([
            TaskSeeder::class,
            DependencySeeder::class,
        ]);
    }
}
```

<% } %>

<% if (isScheduler) { %>

Run the following Artisan commands to create the seeders:

```bash
php artisan make:seeder ResourceSeeder
php artisan make:seeder EventSeeder
php artisan make:seeder AssignmentSeeder
```

Open the `database/seeders/ResourceSeeder.php` file and replace its contents with the following code:

```php
<?php

namespace Database\Seeders;

use Illuminate\Database\Console\Seeds\WithoutModelEvents;
use Illuminate\Database\Seeder;
use App\Models\Resource;

class ResourceSeeder extends Seeder
{
    /**
     * Run the database seeds.
     */
    public function run(): void
    {
        $jsonPath = base_path('example-data/resources.json');

        if (!file_exists($jsonPath)) {
            throw new \Exception("Resources JSON file not found at: {$jsonPath}");
        }

        $resourcesData = json_decode(file_get_contents($jsonPath), true);

        if (json_last_error() !== JSON_ERROR_NONE) {
            throw new \Exception("Invalid JSON in resources file: " . json_last_error_msg());
        }

        foreach ($resourcesData as $resource) {
            Resource::create($resource);
        }
    }
}
```

This seeder reads the resource data from `example-data/resources.json` and inserts each record into the database.

Open the `database/seeders/EventSeeder.php` file and replace its contents with the following code:

```php
<?php

namespace Database\Seeders;

use Illuminate\Database\Console\Seeds\WithoutModelEvents;
use Illuminate\Database\Seeder;
use App\Models\Event;

class EventSeeder extends Seeder
{
    /**
     * Run the database seeds.
     */
    public function run(): void
    {
        $jsonPath = base_path('example-data/events.json');

        if (!file_exists($jsonPath)) {
            throw new \Exception("Events JSON file not found at: {$jsonPath}");
        }

        $eventsData = json_decode(file_get_contents($jsonPath), true);

        if (json_last_error() !== JSON_ERROR_NONE) {
            throw new \Exception("Invalid JSON in events file: " . json_last_error_msg());
        }

        foreach ($eventsData as $event) {
            Event::create($event);
        }
    }
}
```

This seeder reads the event data from `example-data/events.json` and inserts each record into the database.

Open the `database/seeders/AssignmentSeeder.php` file and replace its contents with the following code:

```php
<?php

namespace Database\Seeders;

use Illuminate\Database\Seeder;
use App\Models\Assignment;
use Illuminate\Support\Facades\File;

class AssignmentSeeder extends Seeder
{
    /**
     * Run the database seeds.
     */
    public function run(): void
    {
        $jsonPath = base_path('example-data/assignments.json');

        if (File::exists($jsonPath)) {
            $assignmentsData = json_decode(File::get($jsonPath), true);

            foreach ($assignmentsData as $assignmentData) {
                Assignment::create($assignmentData);
            }
        }
    }
}
```

This seeder reads the assignment data from `example-data/assignments.json` and inserts each record into the database, if
the file exists.

Update `database/seeders/DatabaseSeeder.php`:

```php
<?php

namespace Database\Seeders;

use App\Models\User;
// use Illuminate\Database\Console\Seeds\WithoutModelEvents;
use Illuminate\Database\Seeder;

class DatabaseSeeder extends Seeder
{
    /**
     * Seed the application's database.
     */
    public function run(): void
    {
        $this->call([
            ResourceSeeder::class,
            EventSeeder::class,
            AssignmentSeeder::class,
        ]);
    }
}
```

<% } %>

<% if (isSchedulerPro) { %>

Run the following Artisan commands to create the seeders:

```bash
php artisan make:seeder ResourceSeeder
php artisan make:seeder EventSeeder
php artisan make:seeder AssignmentSeeder
php artisan make:seeder DependencySeeder
```

Open the `database/seeders/ResourceSeeder.php` file and replace its contents with the following code:

```php
<?php

namespace Database\Seeders;

use Illuminate\Database\Console\Seeds\WithoutModelEvents;
use Illuminate\Database\Seeder;
use App\Models\Resource;

class ResourceSeeder extends Seeder
{
    /**
     * Run the database seeds.
     */
    public function run(): void
    {
        $jsonPath = base_path('example-data/resources.json');

        if (!file_exists($jsonPath)) {
            throw new \Exception("Resources JSON file not found at: {$jsonPath}");
        }

        $resourcesData = json_decode(file_get_contents($jsonPath), true);

        if (json_last_error() !== JSON_ERROR_NONE) {
            throw new \Exception("Invalid JSON in resources file: " . json_last_error_msg());
        }

        foreach ($resourcesData as $resource) {
            Resource::create($resource);
        }
    }
}
```

This seeder reads the resource data from `example-data/resources.json` and inserts each record into the database.

Open the `database/seeders/EventSeeder.php` file and replace its contents with the following code:

```php
<?php

namespace Database\Seeders;

use Illuminate\Database\Console\Seeds\WithoutModelEvents;
use Illuminate\Database\Seeder;
use App\Models\Event;

class EventSeeder extends Seeder
{
    /**
     * Run the database seeds.
     */
    public function run(): void
    {
        $jsonPath = base_path('example-data/events.json');

        if (!file_exists($jsonPath)) {
            throw new \Exception("Events JSON file not found at: {$jsonPath}");
        }

        $eventsData = json_decode(file_get_contents($jsonPath), true);

        if (json_last_error() !== JSON_ERROR_NONE) {
            throw new \Exception("Invalid JSON in events file: " . json_last_error_msg());
        }

        foreach ($eventsData as $event) {
            Event::create($event);
        }
    }
}
```

This seeder reads the event data from `example-data/events.json` and inserts each record into the database.

Open the `database/seeders/AssignmentSeeder.php` file and replace its contents with the following code:

```php
<?php

namespace Database\Seeders;

use Illuminate\Database\Console\Seeds\WithoutModelEvents;
use Illuminate\Database\Seeder;
use App\Models\Assignment;

class AssignmentSeeder extends Seeder
{
    /**
     * Run the database seeds.
     */
    public function run(): void
    {
        $jsonPath = base_path('example-data/assignments.json');

        if (!file_exists($jsonPath)) {
            throw new \Exception("Assignments JSON file not found at: {$jsonPath}");
        }

        $assignmentsData = json_decode(file_get_contents($jsonPath), true);

        if (json_last_error() !== JSON_ERROR_NONE) {
            throw new \Exception("Invalid JSON in assignments file: " . json_last_error_msg());
        }

        foreach ($assignmentsData as $assignment) {
            Assignment::create($assignment);
        }
    }
}
```

This seeder reads the assignment data from `example-data/assignments.json` and inserts each record into the database.

Open the `database/seeders/DependencySeeder.php` file and replace its contents with the following code:

```php
<?php

namespace Database\Seeders;

use Illuminate\Database\Console\Seeds\WithoutModelEvents;
use Illuminate\Database\Seeder;
use App\Models\Dependency;

class DependencySeeder extends Seeder
{
    /**
     * Run the database seeds.
     */
    public function run(): void
    {
        $jsonPath = base_path('example-data/dependencies.json');

        if (!file_exists($jsonPath)) {
            throw new \Exception("Dependencies JSON file not found at: {$jsonPath}");
        }

        $dependenciesData = json_decode(file_get_contents($jsonPath), true);

        if (json_last_error() !== JSON_ERROR_NONE) {
            throw new \Exception("Invalid JSON in dependencies file: " . json_last_error_msg());
        }

        foreach ($dependenciesData as $dependency) {
            Dependency::create($dependency);
        }
    }
}
```

This seeder reads the dependency data from `example-data/dependencies.json` and inserts each record into the database.

Update `database/seeders/DatabaseSeeder.php`:

```php
<?php

namespace Database\Seeders;

use App\Models\User;
// use Illuminate\Database\Console\Seeds\WithoutModelEvents;
use Illuminate\Database\Seeder;

class DatabaseSeeder extends Seeder
{
    /**
     * Seed the application's database.
     */
    public function run(): void
    {
        $this->call([
            ResourceSeeder::class,
            EventSeeder::class,
            AssignmentSeeder::class,
            DependencySeeder::class,
        ]);
    }
}
```

<% } %>

<% if (isTaskBoard) { %>

Run the following Artisan commands to create the seeders:

```bash
php artisan make:seeder ResourceSeeder
php artisan make:seeder TaskSeeder
php artisan make:seeder AssignmentSeeder
```

Open the `database/seeders/ResourceSeeder.php` file and replace its contents with the following code:

```php
<?php

namespace Database\Seeders;

use Illuminate\Database\Console\Seeds\WithoutModelEvents;
use Illuminate\Database\Seeder;
use App\Models\Resource;

class ResourceSeeder extends Seeder
{
    /**
     * Run the database seeds.
     */
    public function run(): void
    {
        $jsonPath = base_path('example-data/resources.json');

        if (!file_exists($jsonPath)) {
            throw new \Exception("Resources JSON file not found at: {$jsonPath}");
        }

        $resourcesData = json_decode(file_get_contents($jsonPath), true);

        if (json_last_error() !== JSON_ERROR_NONE) {
            throw new \Exception("Invalid JSON in resources file: " . json_last_error_msg());
        }

        foreach ($resourcesData as $resource) {
            Resource::create($resource);
        }
    }
}
```

This seeder reads the resource data from `example-data/resources.json` and inserts each record into the database.

Open the `database/seeders/TaskSeeder.php` file and replace its contents with the following code:

```php
<?php

namespace Database\Seeders;

use Illuminate\Database\Console\Seeds\WithoutModelEvents;
use Illuminate\Database\Seeder;
use App\Models\Task;

class TaskSeeder extends Seeder
{
    /**
     * Run the database seeds.
     */
    public function run(): void
    {
        $jsonPath = base_path('example-data/tasks.json');

        if (!file_exists($jsonPath)) {
            throw new \Exception("Tasks JSON file not found at: {$jsonPath}");
        }

        $tasksData = json_decode(file_get_contents($jsonPath), true);

        if (json_last_error() !== JSON_ERROR_NONE) {
            throw new \Exception("Invalid JSON in tasks file: " . json_last_error_msg());
        }

        foreach ($tasksData as $task) {
            Task::create($task);
        }
    }
}
```

This seeder reads the task data from `example-data/tasks.json` and inserts each record into the database.

Open the `database/seeders/AssignmentSeeder.php` file and replace its contents with the following code:

```php
<?php

namespace Database\Seeders;

use Illuminate\Database\Console\Seeds\WithoutModelEvents;
use Illuminate\Database\Seeder;
use App\Models\Assignment;

class AssignmentSeeder extends Seeder
{
    /**
     * Run the database seeds.
     */
    public function run(): void
    {
        $jsonPath = base_path('example-data/assignments.json');

        if (!file_exists($jsonPath)) {
            throw new \Exception("Assignments JSON file not found at: {$jsonPath}");
        }

        $assignmentsData = json_decode(file_get_contents($jsonPath), true);

        if (json_last_error() !== JSON_ERROR_NONE) {
            throw new \Exception("Invalid JSON in assignments file: " . json_last_error_msg());
        }

        foreach ($assignmentsData as $assignment) {
            Assignment::create($assignment);
        }
    }
}
```

This seeder reads the assignment data from `example-data/assignments.json` and inserts each record into the database.

Update `database/seeders/DatabaseSeeder.php`:

```php
<?php

namespace Database\Seeders;

use App\Models\User;
// use Illuminate\Database\Console\Seeds\WithoutModelEvents;
use Illuminate\Database\Seeder;

class DatabaseSeeder extends Seeder
{
    /**
     * Seed the application's database.
     */
    public function run(): void
    {
        $this->call([
            ResourceSeeder::class,
            TaskSeeder::class,
            AssignmentSeeder::class,
        ]);
    }
}
```

<% } %>

Now, run the database migrations and seeder. Using `migrate:fresh` ensures that the database is in a clean state every
time you run the command.

```bash
php artisan migrate:fresh --seed
```

This recreates the SQLite database and populates it with the JSON data from the `example-data` directory.

## Creating API endpoints

Let's create the API endpoints that the Bryntum frontend will call to load data and persist changes.

<% if (isGrid) { %>

Run the following Artisan command to create a player controller:

```bash
php artisan make:controller Api/PlayerController
```

Open the `app/Http/Controllers/Api/PlayerController.php` file and replace its contents with the following code:

```php
<?php

namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use Illuminate\Http\Request;
use App\Models\Player;
use Illuminate\Support\Facades\DB;
use Illuminate\Support\Facades\Log;

class PlayerController extends Controller
{
    public function read()
    {
        try {
            $players = Player::all();
            return response()->json([
                'success' => true,
                'data'    => $players,
            ]);
        } catch (\Exception $e) {
            Log::error($e);
            return response()->json([
                'success' => false,
                'message' => 'Players data could not be read.',
            ], 500);
        }
    }

    public function create(Request $request)
    {
        try {
            $data = $request->input('data', []);

            // Perform all creates in a single transaction
            $createdPlayers = DB::transaction(function () use ($data) {
                $result = [];
                foreach ($data as $item) {
                    // Remove id from data as it will be auto-generated
                    unset($item['id']);
                    $player = Player::create($item);
                    $result[] = $player;
                }
                return $result;
            });

            return response()->json([
                'success' => true,
                'data'    => $createdPlayers,
            ]);
        } catch (\Exception $e) {
            Log::error($e);
            return response()->json([
                'success' => false,
                'message' => 'Players could not be created',
            ], 500);
        }
    }

    public function update(Request $request)
    {
        try {
            $data = $request->input('data', []);

            // Perform all updates in a single transaction
            $updatedPlayers = DB::transaction(function () use ($data) {
                $result = [];
                foreach ($data as $item) {
                    $id = $item['id'];
                    unset($item['id']);

                    $player = Player::findOrFail($id);
                    $player->update($item);
                    $result[] = $player;
                }
                return $result;
            });

            return response()->json([
                'success' => true,
                'data'    => $updatedPlayers,
            ]);
        } catch (\Exception $e) {
            Log::error($e);
            return response()->json([
                'success' => false,
                'message' => 'Players could not be updated',
            ], 500);
        }
    }

    public function delete(Request $request)
    {
        try {
            $ids = $request->input('ids', []);

            // Perform the delete operations in a single transaction
            DB::transaction(function () use ($ids) {
                Player::destroy($ids);
            });

            return response()->json(['success' => true]);
        } catch (\Exception $e) {
            Log::error($e);
            return response()->json([
                'success' => false,
                'message' => 'Could not delete selected player record(s)',
            ], 500);
        }
    }
}
```

The `PlayerController` handles CRUD operations for player records. Each method wraps its database operations in a
transaction and returns a JSON response with a `success` flag.

Create a `routes/api.php` file in the `routes` folder and add the following code:

```php
<?php

use App\Http\Controllers\Api\PlayerController;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Route;

Route::get('/read', [PlayerController::class, 'read']);
Route::post('/create', [PlayerController::class, 'create']);
Route::patch('/update', [PlayerController::class, 'update']);
Route::delete('/delete', [PlayerController::class, 'delete']);
```

Open [http://localhost:1337/api/read](http://localhost:1337/api/read) in your browser to verify the data loads correctly
after you start the backend server.

<% } %>

<% if (isCalendar) { %>

Run the following Artisan command to create a controller:

```bash
php artisan make:controller Api/CalendarController
```

Open the `app/Http/Controllers/Api/CalendarController.php` file and replace its contents with the following code:

```php
<?php

namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use Illuminate\Http\Request;
use App\Models\Event;
use App\Models\Resource;
use Illuminate\Support\Facades\DB;
use Illuminate\Support\Facades\Log;

class CalendarController extends Controller
{
    public function load()
    {
        try {
            $events = Event::all();
            $resources = Resource::all();

            $data = json_decode(request()->input('data'), true);

            return response()->json([
                'requestId' => $data['requestId'] ?? null,
                'events' => ['rows' => $events],
                'resources' => ['rows' => $resources],
            ]);
        } catch (\Exception $e) {
            Log::error($e);
            return response()->json([
                'success' => false,
                'message' => 'There was an error loading the events and resources data.',
            ], 500);
        }
    }

    public function sync(Request $request)
    {
        try {
            return DB::transaction(function () use ($request) {
                $requestId = $request->input('requestId');
                $events = $request->input('events');
                $resources = $request->input('resources');

                $response = ['requestId' => $requestId, 'success' => true];

                if ($resources) {
                    $rows = $this->applyTableChanges('resources', $resources);
                    if ($rows) {
                        $response['resources'] = ['rows' => $rows];
                    }
                }

                if ($events) {
                    $rows = $this->applyTableChanges('events', $events);
                    if ($rows) {
                        $response['events'] = ['rows' => $rows];
                    }
                }

                return response()->json($response);
            });
        } catch (\Exception $e) {
            Log::error($e);
            return response()->json([
                'requestId' => $request->input('requestId'),
                'success' => false,
                'message' => 'There was an error syncing the data changes.',
            ], 500);
        }
    }

    private function applyTableChanges($table, $changes)
    {
        $rows = null;

        if (isset($changes['added'])) {
            $rows = $this->createOperation($changes['added'], $table);
        }

        if (isset($changes['updated'])) {
            $this->updateOperation($changes['updated'], $table);
        }

        if (isset($changes['removed'])) {
            $this->deleteOperation($changes['removed'], $table);
        }

        return $rows;
    }

    private function createOperation($added, $table)
    {
        $results = [];

        foreach ($added as $record) {
            $phantomId = $record['$PhantomId'] ?? null;
            unset($record['$PhantomId']);

            if ($table === 'events') {
                $event = Event::create($record);
                $results[] = ['$PhantomId' => $phantomId, 'id' => $event->id];
            } elseif ($table === 'resources') {
                $resource = Resource::create($record);
                $results[] = ['$PhantomId' => $phantomId, 'id' => $resource->id];
            }
        }

        return $results;
    }

    private function updateOperation($updated, $table)
    {
        foreach ($updated as $record) {
            $id = $record['id'];
            unset($record['id']);

            if ($table === 'events') {
                $fillableData = array_intersect_key($record, array_flip((new Event())->getFillable()));
                Event::where('id', $id)->update($fillableData);
            } elseif ($table === 'resources') {
                $fillableData = array_intersect_key($record, array_flip((new Resource())->getFillable()));
                Resource::where('id', $id)->update($fillableData);
            }
        }
    }

    private function deleteOperation($deleted, $table)
    {
        foreach ($deleted as $record) {
            $id = $record['id'];

            if ($table === 'events') {
                Event::where('id', $id)->delete();
            } elseif ($table === 'resources') {
                Resource::where('id', $id)->delete();
            }
        }
    }
}
```

The `CalendarController` has a `load` method that returns all events and resources, and a `sync` method that processes
added, updated, and removed records. It maps phantom IDs to real database IDs for newly created records.

Create a `routes/api.php` file in the `routes` folder and add the following code:

```php
<?php

use App\Http\Controllers\Api\CalendarController;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Route;

Route::get('/load', [CalendarController::class, 'load']);
Route::post('/sync', [CalendarController::class, 'sync']);
```

Open [http://localhost:1337/api/load](http://localhost:1337/api/load) in your browser to verify the data loads correctly
after you start the backend server.

<% } %>

<% if (isGantt) { %>

Run the following Artisan command to create a controller:

```bash
php artisan make:controller Api/TaskController
```

Open the `app/Http/Controllers/Api/TaskController.php` file and replace its contents with the following code:

```php
<?php

namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use Illuminate\Http\Request;
use App\Models\Task;
use App\Models\Dependency;
use Illuminate\Support\Facades\DB;
use Illuminate\Support\Facades\Log;

class TaskController extends Controller
{
    // Bryntum CrudManager load endpoint
    public function load()
    {
        try {
            $tasks = Task::orderBy('parentId', 'ASC')
                ->orderBy('parentIndex', 'ASC')
                ->get();
            $dependencies = Dependency::orderBy('id', 'ASC')->get();

            $data = json_decode(request()->input('data'), true);

            return response()->json([
                'success' => true,
                'requestId' => $data['requestId'] ?? null,
                'revision' => 1,
                'tasks' => [
                    'rows' => $tasks,
                    'total' => $tasks->count(),
                ],
                'dependencies' => [
                    'rows' => $dependencies,
                    'total' => $dependencies->count(),
                ],
            ]);
        } catch (\Exception $e) {
            Log::error($e);
            return response()->json([
                'success' => false,
                'message' => $e->getMessage(),
            ], 500);
        }
    }

    // Bryntum CrudManager sync endpoint
    public function sync(Request $request)
    {
        try {
            return DB::transaction(function () use ($request) {
                $response = [
                    'success' => true,
                    'requestId' => $request->input('requestId') ?? time(),
                    'revision' => ($request->input('revision') ?? 0) + 1,
                    'tasks' => ['rows' => []],
                    'dependencies' => ['rows' => []],
                ];

                $tasks = $request->input('tasks', []);

                // Handle added tasks - map phantom IDs to real IDs
                if (isset($tasks['added'])) {
                    foreach ($tasks['added'] as $task) {
                        $phantomId = $task['$PhantomId'] ?? null;
                        unset($task['$PhantomId']);

                        $newTask = Task::create($task);

                        // Return ID mapping for client to update phantom IDs
                        $mapping = ['id' => $newTask->id];
                        if ($phantomId) {
                            $mapping['$PhantomId'] = $phantomId;
                        }
                        $response['tasks']['rows'][] = $mapping;
                    }
                }

                // Handle updated tasks
                if (isset($tasks['updated'])) {
                    foreach ($tasks['updated'] as $task) {
                        $id = $task['id'];
                        unset($task['id']);

                        $existingTask = Task::find($id);
                        if ($existingTask) {
                            // Update only fields that were sent
                            $fillableData = array_intersect_key($task, array_flip((new Task())->getFillable()));

                            if (!empty($fillableData)) {
                                $existingTask->update($fillableData);
                            }
                        }
                    }
                }

                // Handle removed tasks
                if (isset($tasks['removed'])) {
                    foreach ($tasks['removed'] as $task) {
                        $existingTask = Task::find($task['id']);
                        if ($existingTask) {
                            $existingTask->delete();
                        }
                    }
                }

                // Process dependencies
                $dependencies = $request->input('dependencies', []);

                // Handle added dependencies
                if (isset($dependencies['added'])) {
                    foreach ($dependencies['added'] as $dep) {
                        $phantomId = $dep['$PhantomId'] ?? null;
                        unset($dep['$PhantomId']);

                        $newDep = Dependency::create($dep);

                        // Return ID mapping for client to update phantom IDs
                        $mapping = ['id' => $newDep->id];
                        if ($phantomId) {
                            $mapping['$PhantomId'] = $phantomId;
                        }
                        $response['dependencies']['rows'][] = $mapping;
                    }
                }

                // Handle updated dependencies
                if (isset($dependencies['updated'])) {
                    foreach ($dependencies['updated'] as $dep) {
                        $id = $dep['id'];
                        unset($dep['id']);

                        $existingDep = Dependency::find($id);
                        if ($existingDep) {
                            $fillableData = array_intersect_key($dep, array_flip((new Dependency())->getFillable()));

                            if (!empty($fillableData)) {
                                $existingDep->update($fillableData);
                            }
                        }
                    }
                }

                // Handle removed dependencies
                if (isset($dependencies['removed'])) {
                    foreach ($dependencies['removed'] as $dep) {
                        $existingDep = Dependency::find($dep['id']);
                        if ($existingDep) {
                            $existingDep->delete();
                        }
                    }
                }

                return response()->json($response);
            });
        } catch (\Exception $e) {
            Log::error($e);
            return response()->json([
                'success' => false,
                'message' => $e->getMessage(),
            ], 500);
        }
    }
}
```

The `TaskController` has a `load` method that returns tasks ordered by their hierarchy and dependencies, and a `sync`
method that processes changes to both tasks and dependencies. It maps phantom IDs to real database IDs for newly created
records and tracks revisions.

Create a `routes/api.php` file in the `routes` folder and add the following code:

```php
<?php

use App\Http\Controllers\Api\TaskController;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Route;

Route::get('/load', [TaskController::class, 'load']);
Route::post('/sync', [TaskController::class, 'sync']);
```

Open [http://localhost:1337/api/load](http://localhost:1337/api/load) in your browser to verify the data loads correctly
after you start the backend server.

<% } %>

<% if (isScheduler) { %>

Run the following Artisan command to create a controller:

```bash
php artisan make:controller Api/SchedulerController
```

Open the `app/Http/Controllers/Api/SchedulerController.php` file and replace its contents with the following code:

```php
<?php

namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use Illuminate\Http\Request;
use App\Models\Assignment;
use App\Models\Event;
use App\Models\Resource;
use Illuminate\Support\Facades\DB;
use Illuminate\Support\Facades\Log;

class SchedulerController extends Controller
{
    public function load()
    {
        try {
            $assignments = Assignment::all();
            $events = Event::all();
            $resources = Resource::all();

            $data = json_decode(request()->input('data'), true);

            return response()->json([
                'requestId' => $data['requestId'] ?? null,
                'assignments' => ['rows' => $assignments],
                'events' => ['rows' => $events],
                'resources' => ['rows' => $resources],
            ]);
        } catch (\Exception $e) {
            Log::error($e);
            return response()->json([
                'success' => false,
                'message' => 'There was an error loading the assignments, events, and resources data.',
            ], 500);
        }
    }

    public function sync(Request $request)
    {
        try {
            return DB::transaction(function () use ($request) {
                $requestId = $request->input('requestId');
                $assignments = $request->input('assignments');
                $events = $request->input('events');
                $resources = $request->input('resources');

                $response = ['requestId' => $requestId, 'success' => true];
                $eventMapping = [];

                if ($resources) {
                    $rows = $this->applyTableChanges('resources', $resources);
                    if ($rows) {
                        $response['resources'] = ['rows' => $rows];
                    }
                }

                if ($events) {
                    $rows = $this->applyTableChanges('events', $events);
                    if ($rows) {
                        if (isset($events['added'])) {
                            foreach ($rows as $row) {
                                $eventMapping[$row['$PhantomId']] = $row['id'];
                            }
                        }
                        $response['events'] = ['rows' => $rows];
                    }
                }

                if ($assignments) {
                    if ($events && isset($events['added'])) {
                        foreach ($assignments['added'] as &$assignment) {
                            if (isset($eventMapping[$assignment['eventId']])) {
                                $assignment['eventId'] = $eventMapping[$assignment['eventId']];
                            }
                        }
                    }
                    $rows = $this->applyTableChanges('assignments', $assignments);
                    if ($rows) {
                        $response['assignments'] = ['rows' => $rows];
                    }
                }

                return response()->json($response);
            });
        } catch (\Exception $e) {
            Log::error($e);
            return response()->json([
                'requestId' => $request->input('requestId'),
                'success' => false,
                'message' => 'There was an error syncing the data changes.',
            ], 500);
        }
    }

    private function applyTableChanges($table, $changes)
    {
        $rows = null;

        if (isset($changes['added'])) {
            $rows = $this->createOperation($changes['added'], $table);
        }

        if (isset($changes['updated'])) {
            $this->updateOperation($changes['updated'], $table);
        }

        if (isset($changes['removed'])) {
            $this->deleteOperation($changes['removed'], $table);
        }

        return $rows;
    }

    private function createOperation($added, $table)
    {
        $results = [];

        foreach ($added as $record) {
            $phantomId = $record['$PhantomId'] ?? null;
            unset($record['$PhantomId']);

            if ($table === 'assignments') {
                $assignment = Assignment::create($record);
                $results[] = ['$PhantomId' => $phantomId, 'id' => $assignment->id];
            } elseif ($table === 'events') {
                $event = Event::create($record);
                $results[] = ['$PhantomId' => $phantomId, 'id' => $event->id];
            } elseif ($table === 'resources') {
                $resource = Resource::create($record);
                $results[] = ['$PhantomId' => $phantomId, 'id' => $resource->id];
            }
        }

        return $results;
    }

    private function updateOperation($updated, $table)
    {
        foreach ($updated as $record) {
            $id = $record['id'];
            unset($record['id']);

            if ($table === 'assignments') {
                $fillableData = array_intersect_key($record, array_flip((new Assignment())->getFillable()));
                Assignment::where('id', $id)->update($fillableData);
            } elseif ($table === 'events') {
                $fillableData = array_intersect_key($record, array_flip((new Event())->getFillable()));
                Event::where('id', $id)->update($fillableData);
            } elseif ($table === 'resources') {
                $fillableData = array_intersect_key($record, array_flip((new Resource())->getFillable()));
                Resource::where('id', $id)->update($fillableData);
            }
        }
    }

    private function deleteOperation($deleted, $table)
    {
        foreach ($deleted as $record) {
            $id = $record['id'];

            if ($table === 'assignments') {
                Assignment::where('id', $id)->delete();
            } elseif ($table === 'events') {
                Event::where('id', $id)->delete();
            } elseif ($table === 'resources') {
                Resource::where('id', $id)->delete();
            }
        }
    }
}
```

The `SchedulerController` has a `load` method that returns all events, resources, and assignments, and a `sync` method
that processes changes across all three tables. When new events are created alongside assignments, it maps the event
phantom IDs to real database IDs before creating the assignments.

Create a `routes/api.php` file in the `routes` folder and add the following code:

```php
<?php

use App\Http\Controllers\Api\SchedulerController;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Route;

Route::get('/load', [SchedulerController::class, 'load']);
Route::post('/sync', [SchedulerController::class, 'sync']);
```

Open [http://localhost:1337/api/load](http://localhost:1337/api/load) in your browser to verify the data loads correctly
after you start the backend server.

<% } %>

<% if (isSchedulerPro) { %>

Run the following Artisan command to create a controller:

```bash
php artisan make:controller Api/SchedulerProController
```

Open the `app/Http/Controllers/Api/SchedulerProController.php` file and replace its contents with the following code:

```php
<?php

namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use Illuminate\Http\Request;
use App\Models\Event;
use App\Models\Resource;
use App\Models\Assignment;
use App\Models\Dependency;
use Illuminate\Support\Facades\DB;
use Illuminate\Support\Facades\Log;

class SchedulerProController extends Controller
{
    public function load()
    {
        try {
            $assignments = Assignment::all();
            $dependencies = Dependency::all();
            $events = Event::all();
            $resources = Resource::all();

            $data = json_decode(request()->input('data'), true);

            return response()->json([
                'requestId' => $data['requestId'] ?? null,
                'assignments' => ['rows' => $assignments],
                'dependencies' => ['rows' => $dependencies],
                'events' => ['rows' => $events],
                'resources' => ['rows' => $resources],
            ]);
        } catch (\Exception $e) {
            Log::error($e);
            return response()->json([
                'success' => false,
                'message' => 'There was an error loading the data.',
            ], 500);
        }
    }

    public function sync(Request $request)
    {
        try {
            return DB::transaction(function () use ($request) {
                $requestId = $request->input('requestId');
                $assignments = $request->input('assignments');
                $dependencies = $request->input('dependencies');
                $events = $request->input('events');
                $resources = $request->input('resources');

                $response = ['requestId' => $requestId, 'success' => true];
                $eventMapping = [];

                if ($resources) {
                    $rows = $this->applyTableChanges('resources', $resources);
                    if ($rows) {
                        $response['resources'] = ['rows' => $rows];
                    }
                }

                if ($events) {
                    $rows = $this->applyTableChanges('events', $events);
                    if ($rows) {
                        if (isset($events['added'])) {
                            foreach ($rows as $row) {
                                $eventMapping[$row['$PhantomId']] = $row['id'];
                            }
                        }
                        $response['events'] = ['rows' => $rows];
                    }
                }

                if ($assignments) {
                    if ($events && isset($events['added'])) {
                        foreach ($assignments['added'] as &$assignment) {
                            if (isset($eventMapping[$assignment['eventId']])) {
                                $assignment['eventId'] = $eventMapping[$assignment['eventId']];
                            }
                        }
                    }
                    $rows = $this->applyTableChanges('assignments', $assignments);
                    if ($rows) {
                        $response['assignments'] = ['rows' => $rows];
                    }
                }

                if ($dependencies) {
                    $rows = $this->applyTableChanges('dependencies', $dependencies);
                    if ($rows) {
                        $response['dependencies'] = ['rows' => $rows];
                    }
                }

                return response()->json($response);
            });
        } catch (\Exception $e) {
            Log::error($e);
            return response()->json([
                'requestId' => $request->input('requestId'),
                'success' => false,
                'message' => 'There was an error syncing the data changes: ' . $e->getMessage(),
            ], 500);
        }
    }

    private function applyTableChanges($table, $changes)
    {
        $rows = null;

        if (isset($changes['added'])) {
            $rows = $this->createOperation($changes['added'], $table);
        }

        if (isset($changes['updated'])) {
            $this->updateOperation($changes['updated'], $table);
        }

        if (isset($changes['removed'])) {
            $this->deleteOperation($changes['removed'], $table);
        }

        return $rows;
    }

    private function createOperation($added, $table)
    {
        $results = [];

        foreach ($added as $record) {
            $phantomId = $record['$PhantomId'] ?? null;
            unset($record['$PhantomId']);

            if ($table === 'events') {
                $event = Event::create($record);
                $results[] = ['$PhantomId' => $phantomId, 'id' => $event->id];
            } elseif ($table === 'resources') {
                $resource = Resource::create($record);
                $results[] = ['$PhantomId' => $phantomId, 'id' => $resource->id];
            } elseif ($table === 'assignments') {
                $assignment = Assignment::create($record);
                $results[] = ['$PhantomId' => $phantomId, 'id' => $assignment->id];
            } elseif ($table === 'dependencies') {
                $dependency = Dependency::create($record);
                $results[] = ['$PhantomId' => $phantomId, 'id' => $dependency->id];
            }
        }

        return $results;
    }

    private function updateOperation($updated, $table)
    {
        foreach ($updated as $record) {
            $id = $record['id'];
            unset($record['id']);

            if ($table === 'events') {
                $fillableData = array_intersect_key($record, array_flip((new Event())->getFillable()));
                Event::where('id', $id)->update($fillableData);
            } elseif ($table === 'resources') {
                $fillableData = array_intersect_key($record, array_flip((new Resource())->getFillable()));
                Resource::where('id', $id)->update($fillableData);
            } elseif ($table === 'assignments') {
                $fillableData = array_intersect_key($record, array_flip((new Assignment())->getFillable()));
                Assignment::where('id', $id)->update($fillableData);
            } elseif ($table === 'dependencies') {
                $fillableData = array_intersect_key($record, array_flip((new Dependency())->getFillable()));
                Dependency::where('id', $id)->update($fillableData);
            }
        }
    }

    private function deleteOperation($deleted, $table)
    {
        foreach ($deleted as $record) {
            $id = $record['id'];

            if ($table === 'events') {
                Event::where('id', $id)->delete();
            } elseif ($table === 'resources') {
                Resource::where('id', $id)->delete();
            } elseif ($table === 'assignments') {
                Assignment::where('id', $id)->delete();
            } elseif ($table === 'dependencies') {
                Dependency::where('id', $id)->delete();
            }
        }
    }
}
```

The `SchedulerProController` has a `load` method that returns all events, resources, assignments, and dependencies, and
a `sync` method that processes changes across all four tables. When new events are created alongside assignments, it
maps the event phantom IDs to real database IDs before creating the assignments.

Create a `routes/api.php` file in the `routes` folder and add the following code:

```php
<?php

use App\Http\Controllers\Api\SchedulerProController;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Route;

Route::get('/load', [SchedulerProController::class, 'load']);
Route::post('/sync', [SchedulerProController::class, 'sync']);
```

Open [http://localhost:1337/api/load](http://localhost:1337/api/load) in your browser to verify the data loads correctly
after you start the backend server.

<% } %>

<% if (isTaskBoard) { %>

Run the following Artisan command to create a controller:

```bash
php artisan make:controller Api/TaskBoardController
```

Open the `app/Http/Controllers/Api/TaskBoardController.php` file and replace its contents with the following code:

```php
<?php

namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use Illuminate\Http\Request;
use App\Models\Task;
use App\Models\Resource;
use App\Models\Assignment;
use Illuminate\Support\Facades\DB;
use Illuminate\Support\Facades\Log;

class TaskBoardController extends Controller
{
    public function load()
    {
        try {
            $assignments = Assignment::all();
            $tasks = Task::all();
            $resources = Resource::all();

            $data = json_decode(request()->input('data'), true);

            return response()->json([
                'requestId' => $data['requestId'] ?? null,
                'assignments' => ['rows' => $assignments],
                'tasks' => ['rows' => $tasks],
                'resources' => ['rows' => $resources],
            ]);
        } catch (\Exception $e) {
            Log::error($e);
            return response()->json([
                'success' => false,
                'message' => 'There was an error loading the data.',
            ], 500);
        }
    }

    public function sync(Request $request)
    {
        try {
            return DB::transaction(function () use ($request) {
                $requestId = $request->input('requestId');
                $assignments = $request->input('assignments');
                $tasks = $request->input('tasks');
                $resources = $request->input('resources');

                $response = ['requestId' => $requestId, 'success' => true];

                if ($resources) {
                    $rows = $this->applyTableChanges('resources', $resources);
                    if ($rows) {
                        $response['resources'] = ['rows' => $rows];
                    }
                }

                if ($tasks) {
                    $rows = $this->applyTableChanges('tasks', $tasks);
                    if ($rows) {
                        $response['tasks'] = ['rows' => $rows];
                    }
                }

                if ($assignments) {
                    $rows = $this->applyTableChanges('assignments', $assignments);
                    if ($rows) {
                        $response['assignments'] = ['rows' => $rows];
                    }
                }

                return response()->json($response);
            });
        } catch (\Exception $e) {
            Log::error($e);
            return response()->json([
                'requestId' => $request->input('requestId'),
                'success' => false,
                'message' => 'There was an error syncing the data changes.',
            ], 500);
        }
    }

    private function applyTableChanges($table, $changes)
    {
        $rows = null;

        if (isset($changes['added'])) {
            $rows = $this->createOperation($changes['added'], $table);
        }

        if (isset($changes['updated'])) {
            $this->updateOperation($changes['updated'], $table);
        }

        if (isset($changes['removed'])) {
            $this->deleteOperation($changes['removed'], $table);
        }

        return $rows;
    }

    private function createOperation($added, $table)
    {
        $results = [];

        foreach ($added as $record) {
            $phantomId = $record['$PhantomId'] ?? null;
            unset($record['$PhantomId']);

            if ($table === 'tasks') {
                $task = Task::create($record);
                $results[] = ['$PhantomId' => $phantomId, 'id' => $task->id];
            } elseif ($table === 'resources') {
                $resource = Resource::create($record);
                $results[] = ['$PhantomId' => $phantomId, 'id' => $resource->id];
            } elseif ($table === 'assignments') {
                $assignment = Assignment::create($record);
                $results[] = ['$PhantomId' => $phantomId, 'id' => $assignment->id];
            }
        }

        return $results;
    }

    private function updateOperation($updated, $table)
    {
        foreach ($updated as $record) {
            $id = $record['id'];
            unset($record['id']);

            if ($table === 'tasks') {
                $fillableData = array_intersect_key($record, array_flip((new Task())->getFillable()));
                Task::where('id', $id)->update($fillableData);
            } elseif ($table === 'resources') {
                $fillableData = array_intersect_key($record, array_flip((new Resource())->getFillable()));
                Resource::where('id', $id)->update($fillableData);
            } elseif ($table === 'assignments') {
                $fillableData = array_intersect_key($record, array_flip((new Assignment())->getFillable()));
                Assignment::where('id', $id)->update($fillableData);
            }
        }
    }

    private function deleteOperation($deleted, $table)
    {
        foreach ($deleted as $record) {
            $id = $record['id'];

            if ($table === 'tasks') {
                Task::where('id', $id)->delete();
            } elseif ($table === 'resources') {
                Resource::where('id', $id)->delete();
            } elseif ($table === 'assignments') {
                Assignment::where('id', $id)->delete();
            }
        }
    }
}
```

The `TaskBoardController` has a `load` method that returns all tasks, resources, and assignments, and a `sync` method
that processes added, updated, and removed records across all three tables.

Create a `routes/api.php` file in the `routes` folder and add the following code:

```php
<?php

use App\Http\Controllers\Api\TaskBoardController;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Route;

Route::get('/load', [TaskBoardController::class, 'load']);
Route::post('/sync', [TaskBoardController::class, 'sync']);
```

Open [http://localhost:1337/api/load](http://localhost:1337/api/load) in your browser to verify the data loads correctly
after you start the backend server.

<% } %>

## Setting up the frontend

In a new terminal, create a Vite project:

```bash
npm create vite@latest bryntum-pkgname-frontend -- --template vanilla
cd bryntum-pkgname-frontend
npm install
```

If your Vite template creates extra starter files such as `src/counter.js` or `src/javascript.svg`, you can delete them.

<div class="note">
Laravel includes an official <a href="https://laravel.com/docs/vite">Vite plugin</a> that you can use if you prefer to bundle your frontend assets inside the Laravel app instead of creating a separate Vite frontend.
</div>

### Installing the pkgBryntumTitle component

If you're trying out Bryntum, install the public
[Bryntum trial package](#pkgName/guides/npm/repository/public-repository-access.md). If you have a Bryntum license,
refer to our [npm Repository Guide](#pkgName/guides/npm/repository/private-repository-access.md) and install the
licensed package.

<div class="docs-tabs" data-name="licensed">
<div>
    <a>Trial version</a>
    <a>Licensed version</a>
</div>
<div>

```bash
npm install @bryntum/pkgname@npm:@bryntum/pkgname-trial
```

</div>
<div>

```bash
npm install @bryntum/pkgname
```

</div>
</div>

### Creating and configuring the pkgBryntumTitle frontend

Update `src/main.js`:

<% if (isCalendar) { %>

```js
import { Calendar } from "@bryntum/calendar";
import "./style.css";

const calendar = new Calendar({
  appendTo: "app",
  date: new Date(2026, 6, 20),
  crudManager: {
    loadUrl: "http://localhost:1337/api/load",
    autoLoad: true,
    syncUrl: "http://localhost:1337/api/sync",
    autoSync: true,
    validateResponse: true,
  },
});
```

The `crudManager` configuration connects the calendar to the backend API. The `loadUrl` fetches initial data and the
`syncUrl` handles changes. Setting `autoLoad` and `autoSync` to `true` enables automatic data loading and
synchronization.

<% } %>

<% if (isGantt) { %>

```js
import { Gantt } from "@bryntum/gantt";
import "./style.css";

const gantt = new Gantt({
  appendTo: "app",
  viewPreset: "weekAndDayLetter",
  barMargin: 10,
  project: {
    taskStore: {
      transformFlatData: true,
    },
    loadUrl: "http://localhost:1337/api/load",
    autoLoad: true,
    syncUrl: "http://localhost:1337/api/sync",
    autoSync: true,
    validateResponse: true,
    startedTaskScheduling: "Manual",
  },
  columns: [
    { type: "name", field: "name", text: "Name", width: 250 },
    { type: "startdate", field: "startDate", text: "Start Date" },
    { type: "enddate", field: "endDate", text: "End Date" },
    { type: "duration", field: "fullDuration", text: "Duration" },
    { type: "percentdone", field: "percentDone", text: "% Done", width: 80 },
  ],
});
```

The `project` configuration loads and syncs the task and dependency stores. `taskStore.transformFlatData` converts the
flat task data into the hierarchical structure that Bryntum Gantt expects.

<% } %>

<% if (isScheduler) { %>

```js
import { Scheduler } from "@bryntum/scheduler";
import "./style.css";

const scheduler = new Scheduler({
  appendTo: "app",
  startDate: new Date(2026, 6, 20, 6),
  endDate: new Date(2026, 6, 20, 20),
  viewPreset: "hourAndDay",
  crudManager: {
    loadUrl: "http://localhost:1337/api/load",
    autoLoad: true,
    syncUrl: "http://localhost:1337/api/sync",
    autoSync: true,
    // This config enables response validation and dumping of found errors to the browser console.
    // It's meant to be used as a development stage helper only so please set it to false for production systems.
    validateResponse: true,
  },
  columns: [{ text: "Name", field: "name", width: 130 }],
});
```

The configuration attaches the Scheduler to the `#app` element, sets the visible time range, and connects the
CrudManager to the Laravel API.

<% } %>

<% if (isSchedulerPro) { %>

```js
import { SchedulerPro } from "@bryntum/schedulerpro";
import "./style.css";

const schedulerPro = new SchedulerPro({
  appendTo: "app",
  startDate: new Date(2026, 6, 20, 6),
  endDate: new Date(2026, 6, 20, 20),
  viewPreset: "hourAndDay",
  // A Project holds the data and the calculation engine for Scheduler Pro. It also acts as a CrudManager, allowing loading data into all stores at once
  project: {
    autoLoad: true,
    autoSync: true,
    transport: {
      load: {
        url: "http://localhost:1337/api/load",
      },
      sync: {
        url: "http://localhost:1337/api/sync",
      },
    },
  },
  columns: [{ text: "Name", field: "name", width: 130 }],
});
```

The [project](#SchedulerPro/model/ProjectModel) configuration connects Scheduler Pro to the backend API. The `transport`
object defines the load and sync endpoints for the project stores.

<% } %>

<% if (isGrid) { %>

```js
import { Grid } from "@bryntum/grid";
import { AjaxStore } from "@bryntum/grid";
import "./style.css";

const store = new AjaxStore({
  createUrl: "http://localhost:1337/api/create",
  readUrl: "http://localhost:1337/api/read",
  updateUrl: "http://localhost:1337/api/update",
  deleteUrl: "http://localhost:1337/api/delete",
  autoLoad: true,
  autoCommit: true,
  useRestfulMethods: true,
  httpMethods: {
    read: "GET",
    create: "POST",
    update: "PATCH",
    delete: "DELETE",
  },
});

const grid = new Grid({
  appendTo: "app",
  store,
  columns: [
    { type: "rownumber" },
    {
      text: "Name",
      field: "name",
      width: 280,
    },
    {
      text: "City",
      field: "city",
      width: 220,
    },
    {
      text: "Team",
      field: "team",
      width: 270,
    },
    {
      type: "number",
      text: "Score",
      field: "score",
      width: 100,
    },
    {
      type: "percent",
      text: "Percent wins",
      field: "percentageWins",
      width: 200,
    },
  ],
});
```

This uses the Bryntum Grid [AjaxStore](#Core/data/AjaxStore), which uses the
[Fetch API](https://developer.mozilla.org/en-US/docs/Web/API/Fetch_API/Using_Fetch) to read data from a remote server
and send synchronization requests. The `autoLoad` and `autoCommit` options enable automatic data loading and saving.

<% } %>

<% if (isTaskBoard) { %>

```js
import { TaskBoard } from "@bryntum/taskboard";
import "./style.css";

const taskBoard = new TaskBoard({
  appendTo: "app",

  // Experimental, transition moving cards using the editor
  useDomTransition: true,

  // Columns to display
  columns: [
    { id: "todo", text: "Todo", color: "orange" },
    { id: "doing", text: "Doing", color: "blue", tooltip: "Items that are currently in progress" },
    { id: "done", text: "Done" },
  ],

  // Field used to pair a task to a column
  columnField: "status",

  project: {
    loadUrl: "http://localhost:1337/api/load",
    syncUrl: "http://localhost:1337/api/sync",
    autoLoad: true,
    autoSync: true,
  },
});
```

The [project](#TaskBoard/model/ProjectModel) configuration connects the task board to the backend API. The `loadUrl` and
`syncUrl` properties specify the endpoints for loading and syncing data. The `columnField` and `columns` configuration
define the Kanban columns based on task status.

<% } %>

### Adding styles

Update `src/style.css`:

<% if (isCalendar) { %>

```css
@import "https://fonts.googleapis.com/css2?family=Poppins:wght@300;400;500;600;700&display=swap";
@import "@bryntum/calendar/fontawesome/css/fontawesome.css";
@import "@bryntum/calendar/fontawesome/css/solid.css";
/* Import calendar's structural CSS */
@import "@bryntum/calendar/calendar.css";
/* Import your preferred Bryntum theme */
@import "@bryntum/calendar/svalbard-light.css";

* {
  margin: 0;
}

body,
html {
  font-family: Poppins, "Open Sans", Helvetica, Arial, sans-serif;
}

#app {
  display: flex;
  flex-direction: column;
  height: 100vh;
  font-size: 14px;
}
```

This imports the Bryntum Calendar structural CSS and the Svalbard Light theme. Bryntum components can also be styled
with the other bundled themes or a custom theme.

<% } %>

<% if (isGantt) { %>

```css
@import "https://fonts.googleapis.com/css2?family=Poppins:wght@300;400;500;600;700&display=swap";
@import "@bryntum/gantt/fontawesome/css/fontawesome.css";
@import "@bryntum/gantt/fontawesome/css/solid.css";
/* Import Gantt's structural CSS */
@import "@bryntum/gantt/gantt.css";
/* Import your preferred Bryntum theme */
@import "@bryntum/gantt/svalbard-light.css";

* {
  margin: 0;
}

body,
html {
  font-family: Poppins, "Open Sans", Helvetica, Arial, sans-serif;
}

#app {
  display: flex;
  flex-direction: column;
  height: 100vh;
  font-size: 14px;
}
```

This imports the Bryntum Gantt structural CSS and the Svalbard Light theme. Bryntum components can also be styled with
the other bundled themes or a custom theme.

<% } %>

<% if (isScheduler) { %>

```css
@import "https://fonts.googleapis.com/css2?family=Poppins:wght@300;400;500;600;700&display=swap";
@import "@bryntum/scheduler/fontawesome/css/fontawesome.css";
@import "@bryntum/scheduler/fontawesome/css/solid.css";
/* Import Scheduler's structural CSS */
@import "@bryntum/scheduler/scheduler.css";
/* Import your preferred Bryntum theme */
@import "@bryntum/scheduler/svalbard-light.css";

* {
  margin: 0;
}

body,
html {
  font-family: Poppins, "Open Sans", Helvetica, Arial, sans-serif;
}

#app {
  display: flex;
  flex-direction: column;
  height: 100vh;
  font-size: 14px;
}
```

This imports the Bryntum Scheduler structural CSS and the Svalbard Light theme. Bryntum components can also be styled
with the other bundled themes or a custom theme.

<% } %>

<% if (isSchedulerPro) { %>

```css
@import "https://fonts.googleapis.com/css2?family=Poppins:wght@300;400;500;600;700&display=swap";
@import "@bryntum/schedulerpro/fontawesome/css/fontawesome.css";
@import "@bryntum/schedulerpro/fontawesome/css/solid.css";
/* Import Scheduler Pro's structural CSS */
@import "@bryntum/schedulerpro/schedulerpro.css";
/* Import your preferred Bryntum theme */
@import "@bryntum/schedulerpro/svalbard-light.css";

* {
  margin: 0;
}

body,
html {
  font-family: Poppins, "Open Sans", Helvetica, Arial, sans-serif;
}

#app {
  display: flex;
  flex-direction: column;
  height: 100vh;
  font-size: 14px;
}
```

This imports the Bryntum Scheduler Pro structural CSS and the Svalbard Light theme. Bryntum components can also be
styled with the other bundled themes or a custom theme.

<% } %>

<% if (isGrid) { %>

```css
@import "https://fonts.googleapis.com/css2?family=Poppins:wght@300;400;500;600;700&display=swap";
@import "@bryntum/grid/fontawesome/css/fontawesome.css";
@import "@bryntum/grid/fontawesome/css/solid.css";
/* Import Grid's structural CSS */
@import "@bryntum/grid/grid.css";
/* Import your preferred Bryntum theme */
@import "@bryntum/grid/svalbard-light.css";

* {
  margin: 0;
}

body,
html {
  font-family: Poppins, "Open Sans", Helvetica, Arial, sans-serif;
}

#app {
  display: flex;
  flex-direction: column;
  height: 100vh;
  font-size: 14px;
}
```

This imports the Bryntum Svalbard Light theme. Bryntum components can also be styled with the other bundled themes or a
custom theme.

<% } %>

<% if (isTaskBoard) { %>

```css
@import "https://fonts.googleapis.com/css2?family=Poppins:wght@300;400;500;600;700&display=swap";
@import "@bryntum/taskboard/fontawesome/css/fontawesome.css";
@import "@bryntum/taskboard/fontawesome/css/solid.css";
/* Import Task Board's structural CSS */
@import "@bryntum/taskboard/taskboard.css";
/* Import your preferred Bryntum theme */
@import "@bryntum/taskboard/svalbard-light.css";

* {
  margin: 0;
}

body,
html {
  font-family: Poppins, "Open Sans", Helvetica, Arial, sans-serif;
}

#app {
  display: flex;
  flex-direction: column;
  height: 100vh;
  font-size: 14px;
}

.b-task-board-base {
  --task-board-body-padding: 1em;
  --task-board-column-header-border-top: 5px solid currentColor;
  --task-board-column-header-border-radius: 3px 3px 0 0;
  --task-board-card-border-top: 5px solid currentColor;
}
```

This imports the Bryntum Task Board structural CSS and the Svalbard Light theme. The custom CSS variables adjust the
card and column header styling.

<% } %>

## Running the application

First, start the Laravel backend:

```bash
composer run dev
```

Then, in the frontend project, start the Vite development server:

```bash
npm run dev
```

Open [http://localhost:5173](http://localhost:5173/) in your browser. You'll see a pkgBryntumTitle loading data from the
local SQLite database through the Laravel API.

<video controls width="100%">
<source src="data/pkgName/images/integration/backends/laravel/final-app.webm" type="video/webm">
Sorry, your browser doesn't support embedded videos.
</video>

## Next steps

This tutorial covers the basics of using pkgBryntumTitle with Laravel and SQLite. Take a look at the
[pkgBryntumTitle examples page](https://bryntum.com/products/pkgname/examples/) to browse the additional features you
can add to your app.

You can also explore our other frontend integration guides if you want to use the same Laravel backend with React,
Angular, or Vue.
