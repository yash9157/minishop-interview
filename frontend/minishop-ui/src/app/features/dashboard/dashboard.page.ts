import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';

@Component({
  selector: 'app-dashboard',
  imports: [RouterLink, ButtonModule],
  templateUrl: './dashboard.page.html',
})
export class DashboardPage {}
