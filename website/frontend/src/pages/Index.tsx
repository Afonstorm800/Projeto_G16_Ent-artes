import { useState } from "react";
import AppSidebar from "@/components/AppSidebar";
import DashboardPage from "@/pages/DashboardPage";
import BookingPage from "@/pages/BookingPage";
import ValidationPage from "@/pages/ValidationPage";
import BillingPage from "@/pages/BillingPage";
import InventoryPage from "@/pages/InventoryPage";
import ProfilePage from "@/pages/ProfilePage";
import LoginPage from "@/pages/LoginPage";
import { useAuth } from "@/contexts/AuthContext";

const pages: Record<string, React.ComponentType> = {
  dashboard: DashboardPage,
  booking: BookingPage,
  validation: ValidationPage,
  billing: BillingPage,
  inventory: InventoryPage,
  profile: ProfilePage,
};

const Index = () => {
  const [currentPage, setCurrentPage] = useState("dashboard");
  const { isAuthenticated } = useAuth();

  if (!isAuthenticated) {
    return <LoginPage />;
  }

  const PageComponent = pages[currentPage] || DashboardPage;

  return (
    <div className="flex min-h-screen bg-background">
      <AppSidebar currentPage={currentPage} onNavigate={setCurrentPage} />
      <main className="flex-1 p-8 overflow-y-auto">
        <PageComponent />
      </main>
    </div>
  );
};

export default Index;
