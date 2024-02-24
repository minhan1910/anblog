import { INavData } from '@coreui/angular';

export const navItems: INavData[] = [
  {
    name: 'Dashboard',
    url: '/dashboard',
    iconComponent: { name: 'cil-speedometer' },
    badge: {
      color: 'info',
      text: 'NEW',
    },
    attributes: {
      "policyName": "Permissions.Dashboard.View"
    }
  },
  {
    name: 'Content',
    url: '/content',
    iconComponent: { name: 'cil-puzzle' },
    children: [
      {
        name: 'Post Categories',
        url: '/content/post-categories',
        attributes: {
          "policyName": "Permissions.PostCategories.View"
        }
      },
      {
        name: 'Posts',
        url: '/content/posts',
        attributes: {
          "policyName": "Permissions.Posts.View"
        }
      },
      {
        name: 'Series of Posts',
        url: '/content/series',
        attributes: {
          "policyName": "Permissions.Series.View"
        }
      },
      {
        name: 'Royalty',
        url: '/content/royalty',
        attributes: {
          "policyName": "Permissions.Royalty.View"
        }
      },
    ],
  },
  {
    name: 'System',
    url: '/system',
    iconComponent: { name: 'cil-notes' },
    children: [
      {
        name: 'Roles',
        url: '/system/roles',
        attributes: {
          "policyName": "Permissions.Roles.View"
        }
      },
      {
        name: 'Users',
        url: '/system/users',
        attributes: {
          "policyName": "Permissions.Users.View"
        }
      },
    ],
  },
];
